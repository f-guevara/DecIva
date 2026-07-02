window.decIva = {
    downloadPdf: function (fileName, base64) {
        const link = document.createElement('a');
        link.href = 'data:application/pdf;base64,' + base64;
        link.download = fileName;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    },
    downloadPdfs: function (files) {
        if (!files || files.length === 0) {
            return;
        }
        if (files.length === 1) {
            window.decIva.downloadPdf(files[0].name, files[0].base64);
            return;
        }
        files.forEach(function (file, index) {
            setTimeout(function () {
                window.decIva.downloadPdf(file.name, file.base64);
            }, index * 500);
        });
    }
};
