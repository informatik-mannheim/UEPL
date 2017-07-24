app.controller("mainController", ($scope, $rootScope, $location) =>
{
    $scope.links = [];
    $scope.links.push({ href: "/lecture",  name: "Lecture" });
    $scope.links.push({ href: "/package",  name: "Package" });
    $scope.links.push({ href: "/artifact", name: "Artifact" });

    $rootScope.$on("$routeChangeSuccess", () =>
    {
        $scope.location = $location.url();
    });

    $scope.navClick = (link) =>
    {
        $location.path(link.href);
    };
});