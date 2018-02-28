function sleep(ms)
{
    return new Promise(resolve => setTimeout(resolve, ms));
}

app.controller("lectureController", ($scope, $resource, $location, $routeParams, Lecture, Config, ngToast) =>
{
    $scope.nLecture = { id: "", name: "" };

    let refresh = () =>
    {
        $scope.lectures = Lecture.query();
    }

    refresh();

    $scope.edit = (lecture) => $location.path("/lecture/" + lecture.id);
    $scope.remove = (lecture) => Lecture.delete({ id: lecture.id }, refresh);

    $scope.create = () =>
    {
        var nLecture = new Lecture;
        nLecture.id = $scope.nLecture.id;
        nLecture.name = $scope.nLecture.name;
        nLecture.$save(refresh);
    };
});

app.controller("lectureDetailController", ($scope, $http, $routeParams, $q, $window, $location, Lecture, Package, Config, ngToast) =>
{
    $scope.lecture =
    {
        "content": [],
        "storage": { "files": [] }
    };

    $scope.packages = [];
    $scope.dirty = false;

    let refresh = () =>
    {
        return $q.all([Package.query().$promise, Lecture.get({ id: $routeParams.id }).$promise, $http.get(Config.API + "lecture/" + $routeParams.id + "/file/")])
        .then((result) => 
        {
            $scope.packages = result[0];
            $scope.lecture = result[1];
            $scope.files = result[2].data;

            for(var i = 0; i < $scope.lecture.content.length; i++)
                changeSelection($scope.lecture.content[i].artifactRefID, true);

            $scope.dirty = false;
        });
    };

    $scope.refresh = refresh;

    $scope.clicked = (package, val) =>
    {
        if(val === undefined)
            package.selected = !package.selected;
        else
            package.selected = val;

        $scope.dirty = true;
    };

    $scope.save = () =>
    {
        if (!$scope.dirty) // no changes
            return;
        else
            $scope.dirty = false;

        let test = $scope.packages.filter(val => val.selected);
        let mapped = test.map(val => val.id);
        
        $http.post(Config.API + "lecture/" + $scope.lecture.id + "/assign", mapped)
            .then(res => ngToast.create({
                className: "info notification",
                content: "Data saved...",
                dismissButton: true
            }));
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
            url: Config.API + "lecture/" + $scope.lecture.id + "/file",
            transformRequest: angular.identity,
            headers: { "Content-Type": undefined },
            data: data
        }).then(response =>
        {
            ngToast.create({
                className: "info notification",
                content: "Upload complete!",
                dismissButton: true
            });

        }).catch(error => 
        {
            if (error.status === 403 || error.status === 401)
            {
                ngToast.create({
                    className: "danger notification",
                    content: "Upload Error: Token isn't valid",
                    dismissButton: true
                });

                $location.path("/login");
            }
            else
            {
                ngToast.create({
                    className: "danger notification",
                    content: "Upload Error: " + error.statusText,
                    dismissButton: true
                });
            }
        });
    };

    $scope.removeStorageFile = (storageFile) =>
    {
        $http.delete(storageFile.url)
             .then(result => { $window.location.reload(); })
             .catch(error => { $location.path("/login");  });
    };

    let changeSelection = (name, val) => 
    {
        let idxP = internIndexOf($scope.packages, name);

        if(idxP !== -1)
            $scope.clicked($scope.packages[idxP], val);
    };

    let internIndexOf = (arr, name) =>
    {
        for(var i = 0; i < arr.length; i++)
        {
            if(arr[i].artifactRefID === name)
            {
                return i;
            }
        }

        return -1;
    };

    refresh();
});