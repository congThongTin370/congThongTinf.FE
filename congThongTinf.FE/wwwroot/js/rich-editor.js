// Khởi tạo trình soạn thảo Quill cho nội dung bài báo.
// Quill giữ HTML; khi submit, HTML được ghi vào textarea ẩn để form gửi đi như bình thường.
(function () {
    var host = document.querySelector('[data-rich-editor]');
    if (!host || typeof Quill === 'undefined') {
        return;
    }

    var textarea = host.querySelector('textarea');
    var form = host.closest('form');

    var toolbarOptions = [
        [{ header: [2, 3, false] }],
        ['bold', 'italic'],
        [{ list: 'ordered' }, { list: 'bullet' }],
        ['blockquote'],
        ['link', 'image']
    ];

    var quill = new Quill(host.querySelector('.rich-editor-content'), {
        theme: 'snow',
        modules: { toolbar: toolbarOptions },
        placeholder: 'Viết nội dung bài báo...'
    });

    var tooltips = {
        '.ql-bold': 'In đậm',
        '.ql-italic': 'In nghiêng',
        '.ql-blockquote': 'Trích dẫn',
        '.ql-link': 'Liên kết',
        '.ql-image': 'Chèn ảnh',
        '.ql-header': 'Heading',
        '.ql-list[value="ordered"]': 'Danh sách đánh số',
        '.ql-list[value="bullet"]': 'Danh sách dấu đầu dòng'
    };
    Object.keys(tooltips).forEach(function (selector) {
        host.querySelectorAll(selector).forEach(function (button) {
            button.setAttribute('title', tooltips[selector]);
        });
    });

    // Chèn ảnh bằng URL thay vì base64, để nội dung nhẹ và dễ quản lý
    quill.getModule('toolbar').addHandler('image', function () {
        var url = window.prompt('Nhập địa chỉ ảnh (https://...):', 'https://');
        if (!url) {
            return;
        }
        if (!/^https?:\/\//i.test(url)) {
            window.alert('Ảnh phải là địa chỉ http:// hoặc https://');
            return;
        }
        var range = quill.getSelection(true);
        quill.insertEmbed(range.index, 'image', url, 'user');
    });

    quill.root.innerHTML = textarea.value;

    form.addEventListener('submit', function () {
        var html = quill.root.innerHTML;
        // Quill để lại một đoạn rỗng khi chưa nhập gì
        textarea.value = html === '<p><br></p>' ? '' : html;
    });
})();
