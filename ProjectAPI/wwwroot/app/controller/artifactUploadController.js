app.controller("artifactUploadController", ($scope, $resource, $routeParams, $http, Config) =>
{
    const ArtifactItems = $http.get(Config.API + "artifact/" + $routeParams.id + "/file").then(res => $scope.artifact = res.data);

    let refresh = () =>
    {

    };

    $scope.uploadFiles = () =>
    {
        let fileUpload = $("#files").get(0);
        let files = fileUpload.files;

        let data = new FormData();

        for (let i = 0; i < files.length; i++)
            data.append(files[i].name, files[i]);

        $http({
            method: "POST",
            url: Config.API + "artifact/" + $routeParams.id + "/upload",
            transformRequest: angular.identity,
            headers: { "Content-Type": undefined },
            data: data
        }).then(res => 
        {
            console.log(res);
            refresh();
        }, err =>
        {
            console.error(err);
        });
    };

});