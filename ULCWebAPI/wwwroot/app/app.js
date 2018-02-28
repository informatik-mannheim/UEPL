var app = angular.module("app", ["ngRoute", "ngResource", "ngSanitize", "ngAnimate", "ui.bootstrap", "ngToast"]);

app.config(["$routeProvider", "$locationProvider", "$resourceProvider", 'ngToastProvider', function ($routeProvider, $locationProvider, $resourceProvider, ngToastProvider)
{
    $routeProvider.when("/",
    {
        redirectTo: "/login"
    })
    .when("/login", 
    {
        templateUrl: "app/partials/login.html"
    })
    .when("/lecture",
    {
        templateUrl: "app/partials/lecture.html"
    })
    .when("/lecture/:id",
    {
        templateUrl: "app/partials/lecture.detail.html"
    })
    .when("/package",
    {
        templateUrl: "app/partials/package.html"
    })
    .when("/package/:id",
    {
        templateUrl: "app/partials/package.detail.html"
    })
    .when("/artifact",
    {
        templateUrl: "app/partials/artifact.html"
    })
    .when("/artifact/:id",
    {
        templateUrl: "app/partials/artifact.detail.html"
    })
    .when("/artifact/:id/upload",
    {
        templateUrl: "app/partials/upload.html"
    })
    .otherwise(
    {
        redirectTo: "/lecture"
    });

    ngToastProvider.configure({
        animation: 'fade', // or 'slide'
        verticalPosition: "bottom",
        timeout: 10000
    });
}]);

app.constant("Config",
{
    //API: "http://elke.sr.hs-mannheim.de:10000/api/"
    API: "http://localhost:10000/api/"
});

app.directive('fallbackSrc', function () 
{
    var fallbackSrc = 
    {
        link: function postLink(scope, iElement, iAttrs) 
        {
            iElement.bind('error', function() 
            {
                angular.element(this).attr("src", iAttrs.fallbackSrc);
            });

            if(iAttrs.ngSrc === "")
                iAttrs.$set("src", iAttrs.fallbackSrc);
        }
    };

    return fallbackSrc;
});