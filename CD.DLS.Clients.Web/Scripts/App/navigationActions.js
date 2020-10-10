var navigationActions = (function () {

    // Keep this variable private inside this closure scope
    //var myGrades = [93, 95, 88, 0, 55, 91];


    
            this.length = 8;
            this.timestamp = +new Date;

            var _getRandomInt = function (min, max) {
                return Math.floor(Math.random() * (max - min + 1)) + min;
            }

    this.generate = function () {
        /*
        var d = +new Date;
                var ts = d.toString();
                var parts = ts.split("").reverse();
                var id = "";

                for (var i = 0; i < this.length; ++i) {
                    var index = _getRandomInt(0, parts.length - 1);
                    id += parts[index];
                }

                return id;
                */

        var S4 = function () {
            return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
        };
        return (S4() + S4() + "-" + S4() + "-" + S4() + "-" + S4() + "-" + S4() + S4() + S4());

            }


        


    var openTile = function (code, argument) {
        var tileId = generate();
        var contentId = "tile-content-" + tileId;
        var contentId = "tile-" + tileId;
        var tileTitle = "";
        var url = "";

        switch (code) {
            case "project":
                tileTitle = "Select Project";
                url = "/project";
                break;
            case "overview":
                tileTitle = "Overview";
                url = "/overview";
                break;
            case "stflow":
                tileTitle = "ST Flow";
                url = "/stflow";
                break;
            case "search":
                tileTitle = "Search";
                url = "/search";
                break;
            case "warnings":
                tileTitle = "Warnings";
                url = "/warnings";
                break;
            case "centricbrowser":
                tileTitle = "Lineage Browser";
                url = "/centricbrowser";
                break;
            default:
                tileTitle = "New Tile";
                url = "/";
        }

        


        /*
                             new MenuItem() { Name = "Overview", Action = "overview" },
                    new MenuItem() { Name = "ST Flow", Action = "stflow" },
                    new MenuItem() { Name = "Data Sources", Action = "datasources" },
                    new MenuItem() { Name = "Warnings", Action = "warnings" },
                    new MenuItem() { Name = "Search", Action = "search" }
         */ 


        $("#tiles").prepend(
            "<div class=tile-container id=" + tileId + " style=\"display: none;\">" +
            "<div class=tile-header>" +
            "<span>" + tileTitle + "</span>" +
            "<span class=tile-close>X</span>" +
            "</div>" +
            "<div class=tile-content id=" + contentId + ">Loading...</div></div>");

        $("#" + tileId + ' .tile-close').click(function () {
            $(this).closest('.tile-container').hide("drop", {
                direction: 'down'
            }, function () {
                $(this).remove();
            });
        });
        $("#" + tileId).show('slow');
        /*
        $("#" + contentId).waitMe({
            effect: 'win8_linear',
            text: 'Loading...',
            bg: "rgba(255,255,255,0.7)",
            color: "#333",
            maxSize: 30,
            waitTime: -1,
            textPos: 'vertical',
            fontSize: '',
            source: '',
            onClose: function () { }
        });
        */

        console.log('opening tile ' + code);
        console.log('argument ' + argument);

        if (argument === null) {
            $.get(url + "/default", function (data) {
                //$("#" + contentId).waitMe('hide');
                $("#" + contentId).html(data);
            });
        }
        else {
            $.get(url + "/default?argument1=" + encodeURI(argument), function (data) {
                //$("#" + contentId).waitMe('hide');
                $("#" + contentId).html(data);
            });
        }
        //$("#" + contentId).load(url + "/default");
        
    }

    /*
    var average = function () {
        var total = myGrades.reduce(function (accumulator, item) {
            return accumulator + item;
        }, 0);

        return 'Your average grade is ' + total / myGrades.length + '.';
    };

    var failing = function () {
        var failingGrades = myGrades.filter(function (item) {
            return item < 70;
        });

        return 'You failed ' + failingGrades.length + ' times.';
    };
    */
    // Explicitly reveal public pointers to the private functions 
    // that we want to reveal publicly

    return {
        openTile: openTile,
        generateId: generate
    }
})();

// navigationActions.failing(); // 'You failed 2 times.'
// navigationActions.average(); // 'Your average grade is 70.33333333333333.'



