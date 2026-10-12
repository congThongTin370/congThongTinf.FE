// Trình soạn thảo định dạng đơn giản (không dùng thư viện).
// Người dùng thao tác như soạn văn bản thường; khi lưu, HTML được đổi sang Markdown
// để backend lưu chuỗi nội dung như cũ (ví dụ: ![image](url)).
(function () {
    var host = document.querySelector('[data-rich-editor]');
    if (!host) {
        return;
    }

    var surface = host.querySelector('[data-editor-surface]');
    var hidden = host.querySelector('[data-content-editor]');
    var form = host.closest('form');
    var fileInput = host.querySelector('[data-image-file]');
    var statusEl = form ? form.querySelector('[data-image-status]') : null;
    var uploadUrl = host.getAttribute('data-upload-url');
    var antiForgery = form ? form.querySelector('input[name="__RequestVerificationToken"]') : null;

    function setStatus(text) {
        if (statusEl) {
            statusEl.textContent = text || '';
        }
    }

    // ---------- Markdown -> HTML (để nạp nội dung cũ vào editor) ----------
    // Chỉ xử lý các cú pháp editor này tạo ra; phần còn lại giữ nguyên dưới dạng đoạn văn.
    function escapeHtml(text) {
        return text
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    function inlineToHtml(text) {
        var out = escapeHtml(text);
        out = out.replace(/!\[([^\]]*)\]\((https?:\/\/[^\s)]+)\)/g, '<img src="$2" alt="$1">');
        out = out.replace(/\[([^\]]+)\]\((https?:\/\/[^\s)]+)\)/g, '<a href="$2">$1</a>');
        out = out.replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>');
        out = out.replace(/\*([^*]+)\*/g, '<em>$1</em>');
        return out;
    }

    function markdownToHtml(md) {
        if (!md) {
            return '';
        }
        var lines = md.replace(/\r\n/g, '\n').split('\n');
        var html = [];
        var listTag = null;
        var paragraph = [];

        function closeList() {
            if (listTag) {
                html.push('</' + listTag + '>');
                listTag = null;
            }
        }
        function flushParagraph() {
            if (paragraph.length) {
                html.push('<p>' + paragraph.map(inlineToHtml).join('<br>') + '</p>');
                paragraph = [];
            }
        }

        lines.forEach(function (line) {
            var heading = /^(#{1,3})\s+(.*)$/.exec(line);
            var bullet = /^[-*]\s+(.*)$/.exec(line);
            var ordered = /^\d+\.\s+(.*)$/.exec(line);
            var quote = /^>\s?(.*)$/.exec(line);

            if (!line.trim()) {
                flushParagraph();
                closeList();
                return;
            }
            if (heading) {
                flushParagraph();
                closeList();
                var level = heading[1].length === 1 ? 2 : heading[1].length;
                html.push('<h' + level + '>' + inlineToHtml(heading[2]) + '</h' + level + '>');
                return;
            }
            if (bullet || ordered) {
                flushParagraph();
                var tag = bullet ? 'ul' : 'ol';
                if (listTag !== tag) {
                    closeList();
                    html.push('<' + tag + '>');
                    listTag = tag;
                }
                html.push('<li>' + inlineToHtml((bullet || ordered)[1]) + '</li>');
                return;
            }
            if (quote) {
                flushParagraph();
                closeList();
                html.push('<blockquote><p>' + inlineToHtml(quote[1]) + '</p></blockquote>');
                return;
            }
            closeList();
            paragraph.push(line);
        });

        flushParagraph();
        closeList();
        return html.join('');
    }

    // ---------- HTML -> Markdown (khi lưu) ----------
    function inlineNodesToMd(node) {
        var out = '';
        node.childNodes.forEach(function (child) {
            if (child.nodeType === Node.TEXT_NODE) {
                out += child.textContent;
                return;
            }
            if (child.nodeType !== Node.ELEMENT_NODE) {
                return;
            }
            var tag = child.tagName.toLowerCase();
            if (tag === 'br') {
                out += '\n';
            } else if (tag === 'img') {
                out += '![' + (child.getAttribute('alt') || 'image') + '](' + child.getAttribute('src') + ')';
            } else if (tag === 'strong' || tag === 'b') {
                var inner = inlineNodesToMd(child).trim();
                out += inner ? '**' + inner + '**' : '';
            } else if (tag === 'em' || tag === 'i') {
                var innerEm = inlineNodesToMd(child).trim();
                out += innerEm ? '*' + innerEm + '*' : '';
            } else if (tag === 'a') {
                out += '[' + inlineNodesToMd(child) + '](' + child.getAttribute('href') + ')';
            } else {
                out += inlineNodesToMd(child);
            }
        });
        return out;
    }

    function blockToMd(node) {
        if (node.nodeType === Node.TEXT_NODE) {
            return node.textContent.trim();
        }
        if (node.nodeType !== Node.ELEMENT_NODE) {
            return '';
        }
        var tag = node.tagName.toLowerCase();
        if (tag === 'h1' || tag === 'h2') {
            return '## ' + inlineNodesToMd(node).trim();
        }
        if (tag === 'h3' || tag === 'h4' || tag === 'h5' || tag === 'h6') {
            return '### ' + inlineNodesToMd(node).trim();
        }
        if (tag === 'ul' || tag === 'ol') {
            var index = 0;
            var items = [];
            node.querySelectorAll(':scope > li').forEach(function (li) {
                index++;
                var prefix = tag === 'ul' ? '- ' : index + '. ';
                items.push(prefix + inlineNodesToMd(li).trim());
            });
            return items.join('\n');
        }
        if (tag === 'blockquote') {
            return inlineNodesToMd(node).trim().split('\n').map(function (l) {
                return '> ' + l;
            }).join('\n');
        }
        return inlineNodesToMd(node).trim();
    }

    function htmlToMarkdown() {
        var blocks = [];
        surface.childNodes.forEach(function (node) {
            var md = blockToMd(node);
            if (md) {
                blocks.push(md);
            }
        });
        return blocks.join('\n\n');
    }

    // ---------- Chèn ảnh tại vị trí con trỏ ----------
    function focusSurface() {
        surface.focus();
    }

    function insertHtmlAtCaret(html) {
        focusSurface();
        var selection = window.getSelection();
        if (!selection || selection.rangeCount === 0 || !surface.contains(selection.anchorNode)) {
            surface.insertAdjacentHTML('beforeend', html);
            return;
        }
        var range = selection.getRangeAt(0);
        range.deleteContents();
        var template = document.createElement('template');
        template.innerHTML = html;
        var fragment = template.content;
        var last = fragment.lastChild;
        range.insertNode(fragment);
        if (last) {
            range.setStartAfter(last);
            range.collapse(true);
            selection.removeAllRanges();
            selection.addRange(range);
        }
    }

    function uploadImage(file) {
        setStatus('Đang tải ảnh lên...');
        var data = new FormData();
        data.append('file', file);
        if (antiForgery) {
            data.append('__RequestVerificationToken', antiForgery.value);
        }

        return fetch(uploadUrl, { method: 'POST', body: data, credentials: 'same-origin' })
            .then(function (response) {
                return response.json().catch(function () {
                    return { success: false, message: 'Chưa tải được ảnh. Vui lòng thử lại.' };
                });
            })
            .then(function (result) {
                if (result && result.success && result.url) {
                    var safeUrl = escapeHtml(result.url);
                    insertHtmlAtCaret('<img src="' + safeUrl + '" alt="image"><p><br></p>');
                    setStatus('Đã chèn ảnh vào nội dung.');
                } else {
                    setStatus((result && result.message) || 'Chưa tải được ảnh. Vui lòng thử lại.');
                }
            })
            .catch(function () {
                setStatus('Chưa tải được ảnh. Vui lòng kiểm tra kết nối và thử lại.');
            });
    }

    // ---------- Lệnh thanh công cụ ----------
    function applyBlockFormat(tag) {
        focusSurface();
        document.execCommand('formatBlock', false, '<' + tag + '>');
    }

    function toggleList(type) {
        focusSurface();
        document.execCommand(type === 'ul' ? 'insertUnorderedList' : 'insertOrderedList');
    }

    function toggleQuote() {
        focusSurface();
        document.execCommand('formatBlock', false, '<blockquote>');
    }

    function addLink() {
        var url = window.prompt('Nhập địa chỉ liên kết (http:// hoặc https://)', 'https://');
        if (!url) {
            return;
        }
        if (!/^https?:\/\//i.test(url)) {
            setStatus('Liên kết phải bắt đầu bằng http:// hoặc https://');
            return;
        }
        focusSurface();
        document.execCommand('createLink', false, url);
    }

    host.querySelectorAll('[data-cmd]').forEach(function (button) {
        button.addEventListener('mousedown', function (event) {
            // Giữ nguyên vùng chọn khi bấm nút thanh công cụ
            event.preventDefault();
        });
        button.addEventListener('click', function () {
            var cmd = button.getAttribute('data-cmd');
            switch (cmd) {
                case 'bold':
                case 'italic':
                    focusSurface();
                    document.execCommand(cmd === 'bold' ? 'bold' : 'italic');
                    break;
                case 'h2':
                case 'h3':
                    applyBlockFormat(cmd);
                    break;
                case 'ul':
                case 'ol':
                    toggleList(cmd);
                    break;
                case 'quote':
                    toggleQuote();
                    break;
                case 'link':
                    addLink();
                    break;
                case 'image':
                    if (fileInput) {
                        fileInput.click();
                    }
                    break;
            }
        });
    });

    if (fileInput) {
        fileInput.addEventListener('change', function () {
            var file = fileInput.files && fileInput.files[0];
            fileInput.value = '';
            if (file) {
                uploadImage(file);
            }
        });
    }

    // Dán ảnh trực tiếp từ clipboard
    surface.addEventListener('paste', function (event) {
        var items = event.clipboardData && event.clipboardData.items;
        if (!items) {
            return;
        }
        for (var i = 0; i < items.length; i++) {
            if (items[i].kind === 'file' && /^image\//.test(items[i].type)) {
                event.preventDefault();
                var file = items[i].getAsFile();
                if (file) {
                    uploadImage(file);
                }
                return;
            }
        }
        // Dán văn bản thuần để tránh mang theo định dạng lạ
        var text = event.clipboardData.getData('text/plain');
        if (text !== '') {
            event.preventDefault();
            document.execCommand('insertText', false, text);
        }
    });

    // Nạp nội dung hiện có (Markdown từ server) vào editor
    surface.innerHTML = markdownToHtml(hidden.value);

    if (form) {
        form.addEventListener('submit', function () {
            hidden.value = htmlToMarkdown();
        });
    }
})();
