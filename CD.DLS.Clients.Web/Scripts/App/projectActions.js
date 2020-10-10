var projectActions = (function (projectId) {

    console.log("Opening project " + projectId);

    var selectProject = function (projectId) {

        $('.tile-close').each(function () {
            $(this).closest('.tile-container').hide("drop", {
                direction: 'down'
            }, function () {
                $(this).remove();
            });
        });

        var url = "project/select/" + projectId;
        console.log("Getting URL " + url);
        $.get(url, function (data) {

            console.log("Refreshing main menu");
            $.get("shared/mainmenu", function (data) {

                $("#mainMenu").replaceWith(data);

            });

        });
    }

    return {
        selectProject: selectProject
    }
})();



