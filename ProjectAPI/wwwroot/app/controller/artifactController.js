app.controller("artifactController", ($scope, $resource, $routeParams, Config) =>
{
    const Package   =    $resource(Config.API + "package/:id");
    const Artifact  =    $resource(Config.API + "artifact/:id");

    let refresh = () =>
    {
        let artifacts = Artifact.query(() => {
            $scope.artifacts = artifacts;
        });
    };

    refresh();

    $scope.clicked = (artifact) =>
    {
        //console.debug(artifact.toJSON());
        let singleArtifact = Artifact.get({ id: artifact.id }, () => { console.debug(singleArtifact); });
    };

    $scope.create = (artifact) =>
    {
        var nArtifact = new Artifact;
        nArtifact.id = artifact.id;
        nArtifact.installAction = artifact.installAction;
        nArtifact.removeAction = artifact.removeAction;

        nArtifact.$save(refresh);
    };

    $scope.remove = (artifact) =>
    {
        Artifact.delete({ id: artifact.id }, refresh);
    };
});

app.controller("artifactDetailController", ($scope, $resource, $routeParams, Config) =>
{
    const Package = $resource(Config.API + "package/:id");
    const Artifact = $resource(Config.API + "artifact/:id");

    let refresh = () => 
    {
        let artifacts = Artifact.query(() => 
        {
            $scope.artifacts = artifacts;
        });
    };

    refresh();

    $scope.clicked = (artifact) => 
    {
        //console.debug(artifact.toJSON());
        let singleArtifact = Artifact.get({ id: artifact.id }, () => { console.debug(singleArtifact); });
    };

    $scope.create = (artifact) => 
    {
        var nArtifact = new Artifact;
        nArtifact.id = artifact.id;
        nArtifact.installAction = artifact.installAction;
        nArtifact.removeAction = artifact.removeAction;

        nArtifact.$save(refresh);
    };

    $scope.remove = (artifact) => 
    {
        Artifact.delete({ id: artifact.id }, refresh);
    };
});