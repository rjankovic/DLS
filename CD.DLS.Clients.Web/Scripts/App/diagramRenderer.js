var diagramRenderer = (function () {

    function addToDiagram(diagramObject, newData) {
        console.log('adding to diagram');

        //return;
        var included = false;

        for (i = 0; i < newData.Nodes.length; i++) {
            included = false;

            for (j = 0; j < diagramObject.nodes.length; j++) {
                if (diagramObject.nodes[j].id == newData.Nodes[i].id) {
                    included = true;
                    break;
                }
            }

            if (!included) {
                diagramObject.nodes.push(newData.Nodes[i]);
                diagramObject.nodeDictionary[newData.Nodes[i].id] = newData.Nodes[i];
            }
        }

        console.log('adding links');

        for (i = 0; i < newData.Links.length; i++) {
            included = false;

            for (j = 0; j < newData.Links.lenght; j++) {
                if (diagramObject.links[j].id == newData.Links[j].id) {
                    included = true;
                    break;
                }
            }

            if (!included) {
                var lnk = newData.Links[i];
                diagramObject.links.push({ id: lnk.id, source: diagramObject.nodeDictionary[lnk.source], target: diagramObject.nodeDictionary[lnk.target] });
            }
        }

        console.log('restart after adding to diagram');
        restart(diagramObject);
    }

    function removeNodes(diagramObject, nodesToRemove) {
        console.log('removing nodes');
        console.log(nodesToRemove);
        //return;

        for (i = 0; i < nodesToRemove.length; i++) {
            var nodeId = nodesToRemove[i];
            console.log('Removing links for node ' + nodeId);

            // remove all referencing links
            while (true) {
                var usageFound = false;

                // search for first usage of the link
                for (j = 0; j < diagramObject.links.length; j++) {
                    if (diagramObject.links[j].source.id == nodeId || diagramObject.links[j].target.id == nodeId) {
                        console.log('Found at ' + j);
                        console.log(diagramObject.links[j]);
                        diagramObject.links.splice(j, 1);

                        usageFound = true;
                        break;
                    }
                }

                // link usage not found, quit
                if (!usageFound) {
                    break;
                }
            }

            // remove the node
            for (j = 0; j < diagramObject.nodes.length; j++) {
                if (diagramObject.nodes[j].id == nodeId) {
                    diagramObject.nodes.splice(j, 1);
                    break;
                }
            }
        }

        restart(diagramObject);
    }

    function removeOutboundLinks(diagramObject, nodeId) {
        var nodesToRemove = [];
        for (i = 0; i < diagramObject.links.length; i++) {
            var lnk = diagramObject.links[i];
            if (lnk.source.id == nodeId) {
                nodesToRemove.push(lnk.target.id);
            }
        }

        if (nodesToRemove.length == 0) {
            return;
        }

        for (i = 0; i < nodesToRemove.length; i++) {
            removeOutboundLinks(diagramObject, nodesToRemove[i]);
        }

        removeNodes(diagramObject, nodesToRemove);
    }

    function removeInboundLinks(diagramObject, nodeId) {
        var nodesToRemove = [];
        for (i = 0; i < diagramObject.links.length; i++) {
            var lnk = diagramObject.links[i];
            console.log(lnk);
            if (lnk.target.id == nodeId) {
                nodesToRemove.push(lnk.source.id);
            }
        }

        if (nodesToRemove.length == 0) {
            return;
        }

        for (i = 0; i < nodesToRemove.length; i++) {
            removeInboundLinks(diagramObject, nodesToRemove[i]);
        }

        removeNodes(diagramObject, nodesToRemove);
    }

    function resetDiagram(diagramObject, diagramData) {
        console.log('resetting diagram');

        diagramObject.nodeDictionary = [];

        var i = 0;
        for (i = 0; i < diagramData.Nodes.length; i++) {

            var nodeToAdd = diagramData.Nodes[i];
            diagramObject.nodes.push(nodeToAdd);
            diagramObject.nodeDictionary[diagramData.Nodes[i].id] = diagramData.Nodes[i];
        }

        console.log('done adding nodes');

        diagramObject.links = [];
        for (i = 0; i < diagramData.Links.length; i++) {
            var lnk = diagramData.Links[i];
            diagramObject.links.push({ id: lnk.id, source: diagramObject.nodeDictionary[lnk.source], target: diagramObject.nodeDictionary[lnk.target] });
        }

        restart(diagramObject);
    }

    var renderDiagram = function (diagram, selectCallback, contextMenu) {


        var diagramObject = {};

        console.log("Rendering diagram");
        console.log(diagram);
        var width = 5000;
        var height = 5000;

        var container = $('#' + diagram.DiagramId);

        var svgId = "svg-" + diagram.DiagramId;

        container.html("<svg class=diagram id='" + svgId + "' width='5000' height='5000'></svg>")

        $(container).scrollTop(2000);
        $(container).scrollLeft(2000);

        var diagramObject = {};


        diagramObject.svg = d3.select("#" + svgId);

        diagramObject.svg.append('defs').append('marker')
            .attr('id', 'arrowhead')
            .attr('viewBox', '-0 -5 10 10')
            .attr('refX', 0)
            .attr('refY', 0)
            .attr('orient', 'auto')
            .attr('markerWidth', 4)
            .attr('markerHeight', 4)
            .attr('xoverflow', 'visible')
            .append('svg:path')
            .attr('d', 'M 0,-5 L 10 ,0 L 0,5')
            .style('stroke', 'none');

        diagramObject.nodes = [];
        diagramObject.links = [];
        diagramObject.nodeDictionary = [];


        diagramObject.simulation = d3.forceSimulation(diagramObject.nodes)
            .force("charge", d3.forceManyBody().strength(-1000))
            .force("link", d3.forceLink(diagramObject.links).distance(200))
            .force("center_force", d3.forceCenter(2300, 2300))
            .force("x", d3.forceX())
            .force("y", d3.forceY())
            .alphaTarget(1)
            .on("tick", ticked);


        diagramObject.nodesGroup = diagramObject.svg.append("g")
            .attr("class", "nodes");

        diagramObject.linksGroup = diagramObject.svg.append("g")
            .attr("class", "links");

        diagramObject.drag_handler = d3.drag()
            .on("start", drag_start)
            .on("drag", drag_drag)
            .on("end", drag_end);

        function drag_start(d) {
            if (!d3.event.active) diagramObject.simulation.alphaTarget(0.3).restart();
            d.fx = d.x;
            d.fy = d.y;
        }

        function drag_drag(d) {
            d.fx = d3.event.x;
            d.fy = d3.event.y;
        }

        function drag_end(d) {
            if (!d3.event.active) diagramObject.simulation.alphaTarget(0);
            d.fx = null;
            d.fy = null;
        }

        diagramObject.node = diagramObject.nodesGroup.selectAll("g");
        diagramObject.link = diagramObject.linksGroup.selectAll("path");

        
        /*
        d3.interval(function () {
            removeNodes(diagramObject, [16367838]);

        }, 20000, d3.now());

        d3.interval(function () {

            var graph1 = {
                "Nodes": [
                    { "width": 255.0, "height": 70.0, "name": "[SampleDWH].[dbo].[DimActivity]", "description": "SQL Table", "id": 16367838 }
                ], "Links": [
                    { "id": 3, "source": 16357822, "target": 16367838 },
                    { "id": 2, "source": 16259675, "target": 16367838 }
                ]
            };

            addToDiagram(diagramObject, graph1);

        }, 20000, d3.now() + 10000);
        */

        function ticked() {

            
            diagramObject.node
                .attr("transform", function (d) {

                    //if (typeof d === null) {
                    //    console.log('node undefined!');
                    //    return 'translate(0,0)';
                    //}

                    return "translate(" + (d.x /**/ - d.width / 2 /**/) + "," + (d.y /**/ - d.height / 2 /**/) + ")"
                });

            diagramObject.link
                .attr("d", function (d) {

                    var sourceRectangle = new Rectangle(d.source.width, d.source.height);
                    var targetRectangle = new Rectangle(d.target.width, d.target.height);
                    var endpoints = getLineEndpoints(d.source, d.target, sourceRectangle, targetRectangle);

                    return "M" + endpoints.x1 + "," + endpoints.y1 + "L" + endpoints.x2 + "," + endpoints.y2;

                });
        }

        var activeNode = null;
        
        if (selectCallback != null) {
            console.log('diagram selection active');
            var nodeSelectionContainer = $("#" + svgId);
            console.log(nodeSelectionContainer);
            $(nodeSelectionContainer).on('click', '.diagramNode', function () {
                var rect = $(this)[0];
                var nodeId = $(rect).attr('diagramNodeId');
                console.log('node activated: ' + nodeId);
                if (activeNode !== null) {
                    $(activeNode).css('fill', '#EEE');
                }
                $(this).css('fill', '#CCC');
                activeNode = $(this);
                selectCallback(nodeId);
            });
        }

        if (contextMenu != null) {
            console.log('context menu:');
            console.log(contextMenu);
            $.contextMenu({
                selector: '#' + svgId + ' .diagramNode',
                callback: function (key, options) {
                    if (contextMenu.callback !== null) {

                        console.log('context menu for');
                        console.log(options.$trigger[0]);
                        var nodeId = options.$trigger[0].getAttribute('diagramNodeId');
                        console.log(nodeId);
                        contextMenu.callback(key, nodeId);
                    }
                },
                items: contextMenu.items
                /*
                {
                    "edit": { name: "Edit" },
                    "cut": { name: "Cut" },
                    "copy": { name: "Copy" },
                    "paste": { name: "Paste" },
                    "delete": { name: "Delete" },
                    "sep1": "---------",
                    "quit": { name: "Quit" }
                }
                */
            });
        }

        resetDiagram(diagramObject, diagram);

        restart(diagramObject);
        diagramObject.simulation.alphaTarget(0.3).restart();
        diagramObject.simulation.alphaTarget(0);

        return diagramObject;
    }

    function restart(diagramObject) {

        console.log('running restart');
        // Apply the general update pattern to the nodes.

        console.log('updating nodes');

        diagramObject.node = diagramObject.node.data(diagramObject.nodes, function (d) { return d.id; });
        diagramObject.node.exit().remove();

        diagramObject.node = diagramObject.node
            .enter()
            .append("g")
            .merge(diagramObject.node);

        var newNodes = diagramObject.node.filter(function () {
            return d3.select(this).select("rect").empty();
        })

        newNodes
            .append("rect")
            .attr("width", function (d) { return d.width; })
            .attr("height", function (d) { return d.height; })
            .attr("class", "diagramNode")
            .attr('diagramNodeId', function (d) { return d.id; })
            .merge(diagramObject.node);


        newNodes
            .append("text")
            .attr("class", "diagramNodeName")
            .attr("x", 20)
            .attr("y", 30)
            .text(function (d) {
                //var seconds = (new Date().getTime() / 1000) % 100;
                return d.name; // + ' ' + seconds;
            });

        newNodes
            .append("text")
            .attr("class", "diagramNodeDescription")
            .attr("x", 20)
            .attr("y", 50)
            .text(function (d) {
                return d.description;
            });

        console.log('updating links');

        diagramObject.link = diagramObject.link.data(diagramObject.links);
        diagramObject.link.exit().remove();

        diagramObject.link =
            diagramObject.link
                .enter()
                .append("path")
                .attr('stroke', '#666666')
                .attr('fill', 'transparent')
                .attr('marker-end', 'url(#arrowhead)')
                .merge(diagramObject.link);

        diagramObject.simulation.nodes(diagramObject.nodes);
        diagramObject.simulation.force("link").links(diagramObject.links);

        diagramObject.simulation.alpha(1).restart();

        diagramObject.drag_handler(diagramObject.node);

    }

    function getLineEndpoints(linkSource, linkTarget, sourceTextBox, targetTextBox) {

        var dx = Math.abs(linkSource.x - linkTarget.x);
        var dy = Math.abs(linkSource.y - linkTarget.y);
        var rdx = linkTarget.x - linkSource.x;
        var rdy = linkTarget.y - linkSource.y;

        if (dx <= sourceTextBox.width / 2 + targetTextBox.width / 2 && dy <= sourceTextBox.height / 2 + targetTextBox.height / 2) {
            return new LineEndpoints(linkSource.x, linkSource.y, linkTarget.x, linkTarget.y);
        }

        var endpoints = new LineEndpoints(0, 0, 0, 0);

        // source exit

        // horizontal exit
        if ((dy == 0) || dx / dy > sourceTextBox.width / sourceTextBox.height) {
            // exit right
            if (rdx > 0) {
                endpoints.x1 = linkSource.x + sourceTextBox.width / 2;
            }
            // exit left
            else {
                endpoints.x1 = linkSource.x - sourceTextBox.width / 2;
            }
            endpoints.y1 = linkSource.y + (rdy / dx) * sourceTextBox.width / 2;
        }

        // vertical exit
        else {
            // exit bottom
            if (rdy > 0) {
                endpoints.y1 = linkSource.y + sourceTextBox.height / 2;
            }
            // exit top
            else {
                endpoints.y1 = linkSource.y - sourceTextBox.height / 2;
            }
            endpoints.x1 = linkSource.x + (rdx / dy) * sourceTextBox.height / 2;
        }


        // target entry

        // horizontal entry
        if (dy == 0 || dx / dy > targetTextBox.width / targetTextBox.height) {
            // entry left
            if (rdx > 0) {
                endpoints.x2 = linkTarget.x - targetTextBox.width / 2;
            }
            // entry right
            else {
                endpoints.x2 = linkTarget.x + targetTextBox.width / 2;
            }
            endpoints.y2 = linkTarget.y - (rdy / dx) * targetTextBox.width / 2;
        }

        // vertical entry
        else {
            // entry top
            if (rdy > 0) {
                endpoints.y2 = linkTarget.y - targetTextBox.height / 2;
            }
            // entry bottom
            else {
                endpoints.y2 = linkTarget.y + targetTextBox.height / 2;
            }
            endpoints.x2 = linkTarget.x - (rdx / dy) * targetTextBox.height / 2;
        }

        return endpoints;
    }


    function LineEndpoints(x1, y1, x2, y2) {
        this.x1 = x1;
        this.y1 = y1;
        this.x2 = x2;
        this.y2 = y2;
    }

    function Rectangle(width, height) {
        this.width = width;
        this.height = height;
    }

    return {
        renderDiagram: renderDiagram,
        resetDiagram: resetDiagram,
        addToDiagram: addToDiagram,
        removeNodes: removeNodes,
        removeInboundLinks: removeInboundLinks,
        removeOutboundLinks: removeOutboundLinks
    }
})();
