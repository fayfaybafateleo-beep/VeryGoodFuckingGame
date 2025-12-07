mergeInto(LibraryManager.library, {
    // 函数名：DownloadFile
    // 参数：str (日志内容), fn (文件名)
    DownloadFile: function(str, fn) {
        // 1. 将 Unity 内存中的字符串指针转换为 JS 字符串
        var msg = UTF8ToString(str);
        var filename = UTF8ToString(fn);

        // 2. 创建一个 Blob 对象 (虚拟文件)
        var blob = new Blob([msg], { type: 'text/plain' });

        // 3. 创建一个隐藏的下载链接元素
        var link = document.createElement('a');
        link.style.display = 'none';
        document.body.appendChild(link);

        // 4. 将 Blob 链接到 URL
        if (window.webkitURL != null) {
            // Chrome allows the link to be clicked without actually adding it to the DOM.
            link.href = window.webkitURL.createObjectURL(blob);
        } else {
            // Firefox requires the link to be added to the DOM before it can be clicked.
            link.href = window.URL.createObjectURL(blob);
            link.onclick = function(event) {
                document.body.removeChild(event.target);
            };
            link.style.display = 'none';
            document.body.appendChild(link);
        }

        // 5. 设置文件名并触发点击
        link.download = filename;
        link.click();

        // 6. 清理内存
        window.URL.revokeObjectURL(link.href);
        document.body.removeChild(link);
    }
});