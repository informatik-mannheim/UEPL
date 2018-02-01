app.controller("loginController", ($scope, $http, $rootScope, $routeParams, $location, $interval, Config, UserService, ngToast) =>
{
    $scope.loginButtonDisabled = false;

    checkToken($http, Config).then(res => 
    {
        if (res.status < 400)
        {
            UserService.User(res.data.user);
            $location.path("/lecture");
        }
    });

    $scope.login = () =>
    {
        $scope.loginButtonDisabled = true;
        let user = { name: $scope.loginName, password: $scope.loginPassword };

        return $http.post(Config.API + "account/login", user).then(
        (response) =>
        {
            UserService.Token(response.data.token);
            UserService.User(response.data.user);
            UserService.Valid(response.data.valid);
            UserService.TokenHeartbeat($interval(() => 
            {
                checkToken($http, Config).catch(error =>
                {
                    if ($scope.User)
                        $scope.logout();
                });
            }, 2*60*1000));

            $http.defaults.headers.common.Token = UserService.Token();

            ngToast.create("Login successful! Welcome " + response.data.user.userName + "...");

            $location.path("/lecture");
        },
        (response) =>
        {
            /*error*/
            if (response.status == 500)
            {
                console.log(response);
                ngToast.create({ className: "danger", content: "Unknown Server Error!" });
            }
            else
            {
                ngToast.create({ className: "danger", content: "Error during login: " + response.statusText });
            }

            $scope.loginButtonDisabled = false;
        });
    };
});