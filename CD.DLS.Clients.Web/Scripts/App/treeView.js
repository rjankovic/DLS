var treeView = (function () {
    
    var showTree = function (container, data, name, selectedCallback) {
        var newId = navigationActions.generateId().toString();
        var treeId = "tree-" + newId;
        var searchId = "search-" + treeId;

        console.log('Rendering tree ' + newId + '[' + treeId + ', ' + searchId + ']');

        var html =
            '<div class="treeSearch">' + name + ': <input type="text" id="' + searchId + '" paceholder="search..."></input></div>'
            + '<div id="' + treeId + '"></div>';

        $(container).append(html);

        $('#' + treeId).jstree(
            {
                'core': {
                    'data': data
                },
                "plugins": ["search"]
            }
        );

        $('#' + treeId).on("changed.jstree", function (e, data) {
            selectedCallback(data.selected);
        });

        var to = false;
        $('#' + searchId).keyup(function () {
            if (to) { clearTimeout(to); }
            to = setTimeout(function () {
                var v = $('#' + searchId).val();
                $('#' + treeId).jstree(true).search(v);
            }, 250);
        });
    }
            
    return {
        showTree: showTree
    }
})();


