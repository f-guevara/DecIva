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
        files.forEach(function (file, index) {
            setTimeout(function () {
                window.decIva.downloadPdf(file.name, file.base64);
            }, index * 500);
        });
    }
};
