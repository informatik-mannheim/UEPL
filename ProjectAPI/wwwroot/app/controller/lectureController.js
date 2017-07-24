function sleep(ms)
{
    return new Promise(resolve => setTimeout(resolve, ms));
}

app.controller("lectureController", ($scope, $resource, $location, $routeParams, Config) =>
{
    $scope.nLecture = { id: "", name: "" };

    const Lecture = $resource(Config.API + "lecture/:id");

    //let testLecture = Lecture.get({ id: "TPE" }, () => { console.log("Single Item:"); console.log(testLecture); });

    let refresh = () =>
    {
        let lectures = Lecture.query(() => {
            $scope.lectures = lectures;

            //lectures.forEach((lect) => { console.debug(lect); });
        });
    }

    refresh();

    $scope.edit = (lecture) =>
    {
        console.log(lecture.toJSON());
        let singleLect = Lecture.get({ id: lecture.id }, () => { $location.path("/lecture/" + lecture.id); });
    };

    $scope.remove = (lecture) =>
    {
        Lecture.delete({ id: lecture.id }, refresh);
    };

    $scope.create = () =>
    {
        var nLecture = new Lecture;
        nLecture.id = $scope.nLecture.id;
        nLecture.name = $scope.nLecture.name;
        nLecture.$save(refresh);
    };
});

app.controller("lectureDetailController", ($scope, $resource, $location, $routeParams, Config) =>
{
    const Lecture = $resource(Config.API + "lecture/:id");
    const Package = $resource(Config.API + "package/:id");

    $scope.selections = [];
    $scope.lecture = Lecture.get({ id: $routeParams.id }, async (lecture) =>
    {
        console.log(lecture);
        console.log(lecture.content.length);

        lecture.content.forEach(obj =>
        {
            console.log(obj);

            $scope.packages.forEach(pack =>
            {
                console.log(pack);

                if (obj.artifactRefID === pack.artifactRefID)
                    $scope.selections.push(pack);
            });
        });

        await sleep(100);

        $("#packageSelector").bootstrapDualListbox("refresh");
    });

    let refresh = () =>
    {
        let packages = Package.query(() =>
        {
            $scope.packages = packages;
        });
    }

    $scope.edit = (lecture) =>
    {
        console.log(lecture.toJSON());
        let singleLect = Lecture.get({ id: lecture.id }, () => { $location.path("/lecture/" + lecture.id); });
    };

    // TODO: Attach & Detach Packages
    // TODO: Maybe Clear Attached Packages and reattach them

    $scope.printScope = () =>
    {
        console.log($scope);
    };

    $scope.printSelection = () =>
    {
        console.log($scope.selections);
    };

    refresh();
});