var app = angular.module("app", ["ngRoute", "ngResource", "ngSanitize", "ngAnimate", "ui.bootstrap", "frapontillo.bootstrap-duallistbox"]);

app.config(["$routeProvider", "$locationProvider", "$resourceProvider", function ($routeProvider, $locationProvider, $resourceProvider)
{
    $routeProvider.when("/",
    {
        redirectTo: "/lecture"
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
}]);

app.constant("Config",
{
    API: "http://localhost:10000/api/"
});