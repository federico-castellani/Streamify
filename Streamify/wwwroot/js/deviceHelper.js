window.browser = {
    getInnerWidth: function () {
        return window.innerWidth;
    },
    onResize: function (dotnetHelper) {
        window.addEventListener('resize', () => {
            dotnetHelper.invokeMethodAsync('OnBrowserResize', window.innerWidth);
        });
    }
}