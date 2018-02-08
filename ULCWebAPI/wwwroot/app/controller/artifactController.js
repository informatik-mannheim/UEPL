app.controller("artifactController", ($scope, $resource, $routeParams, $location, Package, Artifact, Config) =>
{
    let refresh = () =>
    {
        let artifacts = Artifact.query(() => {
            $scope.artifacts = artifacts;
        });
    };

    refresh();

    $scope.edit = (artifact) => 
    {
        $location.path("/artifact/" + artifact.id);
    };

    $scope.create = (artifact) =>
    {
        var nArtifact = new Artifact;
        nArtifact.id = artifact.id;
        nArtifact.installAction = artifact.installAction === undefined ? "# install script" : artifact.installAction;
        nArtifact.removeAction = artifact.removeAction === undefined ? "# remove script" : artifact.removeAction;
        nArtifact.switchAction = artifact.switchAction === undefined ? "# switch script" : artifact.switchAction;
        nArtifact.unswitchAction = artifact.unswitchAction === undefined ? "# unswitch script" : artifact.unswitchAction;

        nArtifact.$save(refresh);
    };

    $scope.remove = (artifact) =>
    {
        Artifact.delete({ id: artifact.id }, refresh);
    };

    $scope.clear = (nArtifact) =>
    {
        nArtifact.id = "";
        nArtifact.installAction = "";
        nArtifact.removeAction = "";
        nArtifact.switchAction = "";
        nArtifact.unswitchAction = "";
    };
});

app.controller("artifactDetailController", ($scope, $resource, $routeParams, $http, $window, $location, Artifact, Config, ngToast) =>
{
    let saveButton = document.getElementById("save");
    saveButton.disabled = true;

    let refreshFiles = () =>
    {
        const ArtifactItems = $http.get(Config.API + "artifact/" + $routeParams.id + "/file").then(res => $scope.artifact.storage = res.data);
    };

    $scope.artifact = Artifact.get({id: $routeParams.id}, (artifact) => 
    {
        editors.ia.setValue(artifact.installAction);
        editors.ra.setValue(artifact.removeAction);
        editors.sa.setValue(artifact.switchAction);
        editors.ua.setValue(artifact.unswitchAction);

        editors.ia.selection.clearSelection();
        editors.ra.selection.clearSelection();
        editors.sa.selection.clearSelection();
        editors.ua.selection.clearSelection();

        editors.ia.resize();
        editors.ra.resize();
        editors.sa.resize();
        editors.ua.resize();

        editors.ia.getSession().on('change', (e) => { saveButton.disabled = false; });
        editors.ra.getSession().on('change', (e) => { saveButton.disabled = false; });
        editors.sa.getSession().on('change', (e) => { saveButton.disabled = false; });
        editors.ua.getSession().on('change', (e) => { saveButton.disabled = false; });

        refreshFiles();
    });

    $scope.uploadFiles = () => 
    {
        let fileUpload = $("#files").get(0);
        let files = fileUpload.files;

        let data = new FormData();

        for (let i = 0; i < files.length; i++) 
            data.append(files[i].name, files[i]);

        $http(
        {
            method: "POST",
            url: Config.API + "artifact/" + $scope.artifact.id + "/file",
            transformRequest: angular.identity,
            headers: { "Content-Type": undefined },
            data: data
        }).then(
        response =>
        {
            ngToast.create({
                className: "info notification",
                content: "Upload complete!",
                dismissButton: true
            });

        }, 
        error =>
        {
            if (error.status === 403 || error.status === 401)
            {
                ngToast.create({
                    className: "error notification",
                    content: "Upload Error: Token isn't valid",
                    dismissButton: true
                });
                $location.path("/login");
            }
            else
            {
                ngToast.create({
                    className: "error notification",
                    content: "Upload Error: " + error.statusText,
                    dismissButton: true
                });
            }
        });
    };

    $scope.save = () => 
    {
        saveButton.disabled = true;
        let cArtifact = Artifact.get({id: $routeParams.id});

        $scope.artifact.installAction = editors.ia.getValue();
        $scope.artifact.removeAction = editors.ra.getValue();
        $scope.artifact.switchAction = editors.sa.getValue();
        $scope.artifact.unswitchAction = editors.ua.getValue();

        $scope.artifact.$update(res =>
        {
            ngToast.create({
                className: "info notification",
                content: "Scripts updated!",
                dismissButton: true
            });
        });
    };

    $scope.removeStorageFile = (storageFile) =>
    {
        $http.delete(storageFile.url).then(result => { $window.location.reload(); }, error =>
        {
            $location.path("/login");
        });
    };

    $scope.cancel = (artifact) => 
    {
        Artifact.delete({ id: artifact.id }, refresh);
    };
});