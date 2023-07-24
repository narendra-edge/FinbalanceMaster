const { createProxyMiddleware } = require('http-proxy-middleware');

const context = [
    "/api/schemeapi",

];

module.exports = function (app) {
    const appProxy = createProxyMiddleware(context, {
        target: 'https://localhost:5445',
        secure: false
    });

    app.use(appProxy);
};
