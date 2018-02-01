function checkToken($http, Config)
{
    return $http.get(Config.API + "account/validate");
}

app.controller("mainController", ($scope, $http, $location, $interval, Config, UserService) =>
{
    $scope.location = $location.url();

    $scope.links = [];
    $scope.links.push({ href: "/lecture",  name: "Lecture" });
    $scope.links.push({ href: "/package",  name: "Package" });
    $scope.links.push({ href: "/artifact", name: "Artifact" });

    $scope.fallbackImage = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAOEAAADhCAMAAAAJbSJIAAAAMFBMVEXp7vG6vsG3u77s8fTCxsnn7O/f5OfP09bFyczM0dO8wMPk6ezY3eDh5unJzdDR1tlr0sxZAAACVUlEQVR4nO3b23KDIBRA0QgmsaLx//+2KmPi/YJMPafZ6619sOzARJjq7QYAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAuJyN4+qMZcUri+BV3WQ22iIxSRTGFBITbRGpr218Ckx0EQPrxMfVPRP25QvNaT4xFTeJ1g/sJ4K8/aTuVxdNNJ99/Q0RQWlELtN7xGH9+8KYH1ZEX1hY770C9186Cm2R1TeONGj/paHQury7OwbsvzQUlp/9jakOJ2ooPLf/kl9on4Mtan50EhUUDvfgh8cqv/AxKlw+Cc3vPeUXjg+Kr4VCm+Vbl5LkeKHNTDKbKL9w3yr1B8q5RPmFu75puhPzTKKCwh13i2aJJguJ8gt33PG7GZxN1FC4tWvrB04TNRRu7Lw/S3Q2UUPh+ulpOIPTRB2FKyfgaeAoUUvhkvESnSYqL5ybwVGi7sKlwH6i6sL5JTpKVFZYlr0flmewTbyvX+piC8NyiXHvH9YD37OoqtA1v+wS15ZofxY1FTo/cJ+4NYNJd9BSVOi6kTeJOwLVFbrPyJ3dXqL6Cl1/7G7HDGordMOx7+hTVui2arQXBgVqKgwLVFQYGKinMDRQTWFwoJrC8AfcKLwUhRRSeL3vKkyDVaNLSdIf1snXEBQUyrlUTBQeIbPQD6uK8Zx3+yyHKbf/5N+y/gn78K/Rj/ZmY64Omhg9gHFaJu59i+EDGKf1/tshRxlxEoW+2uXS868EeflDYmDNltUzgkpqXyPGzULyK6QAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA8DV+AUrRI7QWHsWNAAAAAElFTkSuQmCC";

    $http.defaults.headers.common.Token = UserService.Token();

    $scope.$on("$routeChangeSuccess", () =>
    {
        $scope.location = $location.url();

        if ($location.url() === "/login")
            return;
        
        $scope.User = UserService.User();
    });

    $scope.navClick = (link) =>
    {
        $location.path(link.href);
    };

    $scope.logout = () => 
    {
        delete $http.defaults.headers.common.Token;
        delete $scope.User;
        UserService.Clear();

        let interval = UserService.TokenHeartbeat();

        if (interval)
        {
            $interval.cancel(interval);
            UserService.TokenHeartbeat(undefined);
        }

        $location.path("/login");
        
    };
});