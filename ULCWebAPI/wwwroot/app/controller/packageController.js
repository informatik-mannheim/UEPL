app.controller("packageController", ($scope, $location, $routeParams, $resource, Package, Artifact, Config) =>
{
    $scope.packages = [{ id: 1, name: "not loaded!" }];
    $scope.artifacts = [{ id: 1, name: "not loaded!" }];
    $scope.nPackage = {};
    $scope.dependencies = [];

    $scope.refresh = () => 
    {
        let packages = Package.query(() => {
            $scope.packages = packages;
        });

        let artifacts = Artifact.query(() => {
            $scope.artifacts = artifacts;
        });
    };

    $scope.edit = (package) => 
    {
        $location.path("/package/" + package.id);
    };

    $scope.clickDep = (dependency) => 
    {
        if(dependency.selected)
        {
            let idx = $scope.dependencies.indexOf(dependency);
            
            if(idx > -1)
                $scope.dependencies.splice(idx, 1);
        }
        else
            $scope.dependencies.push(dependency);

        dependency.selected = !dependency.selected;        
    };

    $scope.create = (package) => 
    {
        let nPackage = new Package;
        nPackage.name = package.name;
        nPackage.artifactRefID = package.artifactRefID.id;

        nPackage.$save((pack, response) => { $scope.refresh(); });
    };

    $scope.remove = (package) =>
    {
        Package.delete({ id: package.id }, refresh);
    };

    $scope.refresh();
});

app.controller("packageDetailController", ($scope, $routeParams, $resource, Package, Artifact, Config) =>
{
    $scope.packages = [];
    $scope.artifacts = [];
    $scope.dependencies = [];

    $scope.package = {};
    $scope.artifact = {};

    $scope.clickDep = (dependency) => 
    {
        if(dependency.selected)
        {
            let idx = $scope.dependencies.indexOf(dependency);
            
            if(idx > -1)
                $scope.dependencies.splice(idx, 1);
        }
        else
            $scope.dependencies.push(dependency);

        dependency.selected = !dependency.selected;        
    };

    $scope.refresh = () => 
    {
        return Promise.all([
            Artifact.query().$promise.then(artifacts => $scope.artifacts = artifacts),
            Package.query().$promise.then(packages => $scope.packages = packages),
            Package.get({ id: $routeParams.id }).$promise.then(item => 
            {
                $scope.package = item;
                $scope.artifact = item.artifactRefID;
            })
        ]);
    };

    $scope.refresh();
    
});