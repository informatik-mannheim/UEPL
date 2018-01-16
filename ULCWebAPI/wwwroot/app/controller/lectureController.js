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

app.controller("lectureDetailController", ($scope, $http, $routeParams, $q, Lecture, Package, Config, ngToast) =>
{
    $scope.lecture = {"content": []};
    $scope.packages = [];
    $scope.dirty = false;

    let refresh = () =>
    {
        return $q.all([Package.query().$promise, Lecture.get({id: $routeParams.id}).$promise])
        .then((result) => 
        {
            $scope.packages = result[0];
            $scope.lecture = result[1];

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