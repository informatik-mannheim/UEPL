app.controller("packageController", ($scope, $rootScope, $routeParams, $resource, Config) =>
{
    const Lecture = $resource(Config.API + "lecture/:id");
    const Package = $resource(Config.API + "package/:id");

    $scope.packages = [{ id: 1, name: "not loaded!" }];
    $scope.selections = [];

    if ($routeParams.id !== undefined)
    {
        let item = Lecture.get({ id: $routeParams.id }, () => 
        {
            $scope.lecture = item;
            console.log($scope.lecture);
        });

        let packages = Package.query(() => 
        {
            $scope.packages = packages;
        });
    }
    else
    {
        let refresh = () => 
        {
            let packages = Package.query(() => {
                $scope.packages = packages;
            });
        };

        refresh();

        $scope.clicked = (package) => 
        {
            console.debug(package.toJSON());
            let singlePackage = Package.get({ id: package.id }, () => { /*console.debug(singlePackage);*/ });
        };

        $scope.remove = (package) =>
        {
            Package.delete({ id: package.id }, refresh);
        };
    }
});

app.controller("packageDetailController", ($scope, $rootScope, $routeParams, $resource, Config) =>
{
    const Lecture = $resource(Config.API + "lecture/:id");
    const Package = $resource(Config.API + "package/:id");

    $scope.packages = [{ id: 1, name: "not loaded!" }];
    $scope.selections = [];

    if ($routeParams.id !== undefined)
    {
        let item = Lecture.get({ id: $routeParams.id }, () => 
        {
            $scope.lecture = item;
            console.log($scope.lecture);
        });

        let packages = Package.query(() => 
        {
            $scope.packages = packages;
        });
    }
    else
    {
        let refresh = () => 
        {
            let packages = Package.query(() =>
            {
                $scope.packages = packages;
            });
        };

        refresh();

        $scope.clicked = (package) => 
        {
            console.debug(package.toJSON());
            let singlePackage = Package.get({ id: package.id }, () => { /*console.debug(singlePackage);*/ });
        };

        $scope.remove = (package) =>
        {
            Package.delete({ id: package.id }, refresh);
        };
    }
});