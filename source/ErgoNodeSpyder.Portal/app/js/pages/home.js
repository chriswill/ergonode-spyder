(function () {

    var updateMap = function (items) {
        $.get(`/api/nodes/geo/countries?count=${items}`, function (data) {
            const countries = data.data.map(x => {
                return x.code;
            });

            // Set themes
            // https://www.amcharts.com/docs/v5/concepts/themes/
            root.setThemes([am5themes_Animated.new(root)]);

            // Create the map chart
            // https://www.amcharts.com/docs/v5/charts/map-chart/
            var chart = root.container.children.push(
                am5map.MapChart.new(root, {
                    panX: 'translateX',
                    panY: 'translateY',
                    projection: am5map.geoMercator(),
                    paddingLeft: 0,
                    paddingRight: 0,
                    paddingBottom: 0
                })
            );

            // Create main polygon series for countries
            // https://www.amcharts.com/docs/v5/charts/map-chart/map-polygon-series/
            var polygonSeries = chart.series.push(
                am5map.MapPolygonSeries.new(root, {
                    geoJSON: am5geodata_worldLow,
                    exclude: ['AQ']
                })
            );

            polygonSeries.mapPolygons.template.setAll({
                tooltipText: '{name}',
                toggleKey: 'active',
                interactive: true,
                fill: am5.color(KTUtil.getCssVariableValue('--bs-gray-300'))
            });

            polygonSeries.mapPolygons.template.states.create('hover', {
                fill: am5.color(KTUtil.getCssVariableValue('--bs-success'))
            });

            polygonSeries.mapPolygons.template.states.create('active', {
                fill: am5.color(KTUtil.getCssVariableValue('--bs-success'))
            });

            // Highlighted Series
            // Create main polygon series for countries
            // https://www.amcharts.com/docs/v5/charts/map-chart/map-polygon-series/
            var polygonSeriesHighlighted = chart.series.push(
                am5map.MapPolygonSeries.new(root, {
                    geoJSON: am5geodata_worldLow,
                    include: countries
                })
            );

            polygonSeriesHighlighted.mapPolygons.template.setAll({
                tooltipText: '{name}',
                toggleKey: 'active',
                interactive: true
            });

            var colors = am5.ColorSet.new(root, {});

            polygonSeriesHighlighted.mapPolygons.template.set(
                'fill',
                am5.color(KTUtil.getCssVariableValue('--bs-primary'))
            );

            polygonSeriesHighlighted.mapPolygons.template.states.create("hover", {
                fill: root.interfaceColors.get('primaryButtonHover')
            });

            polygonSeriesHighlighted.mapPolygons.template.states.create("active", {
                fill: root.interfaceColors.get('primaryButtonHover')
            });

            // Add zoom control
            // https://www.amcharts.com/docs/v5/charts/map-chart/map-pan-zoom/#Zoom_control
            //chart.set("zoomControl", am5map.ZoomControl.new(root, {}));

            // Set clicking on "water" to zoom out
            chart.chartContainer
                .get('background')
                .events.on('click', function () {
                    chart.goHome();
                });

            // Make stuff animate on load
            chart.appear(1000, 100);
        });
    }

    var updateDailyNodeCount = function () {
        $.get('/api/nodes/daily-count', function (data) {
            const length = data.data.length;

            if (length > 0) {
                const val = data.data[length - 1];
                $('#daily-node-count').text(val.value);
                const keys = data.data.map(x => {
                    return x.key;
                });
                const values = data.data.map(x => {
                    return x.value;
                });

                var element = document.getElementById('daily-node-display');

                initNodeChart(element, keys, values);
            }
        });
    }

    var updateWeeklyNodeCount = function () {
        $.get('/api/nodes/weekly-count', function (data) {
            const length = data.data.length;

            if (length > 0) {
                const val = data.data[length - 1];
                $('#weekly-node-count').text(val.value);
                const keys = data.data.map(x => {
                    return x.key;
                });
                const values = data.data.map(x => {
                    return x.value;
                });

                var element = document.getElementById('weekly-node-display');

                initNodeChart(element, keys, values);
            }
        });
    }

    var updateMonthlyNodeCount = function () {
        $.get('/api/nodes/monthly-count', function (data) {
            const length = data.data.length;

            if (length > 0) {
                const val = data.data[length - 1];
                $('#monthly-node-count').text(val.value);
                const keys = data.data.map(x => {
                    return x.key;
                });
                const values = data.data.map(x => {
                    return x.value;
                });
                var element = document.getElementById('monthly-node-display');
                initNodeChart(element, keys, values);
            }
        });
    }
    
    var getVersions = function (showCount, update) {
        $.get(`/api/nodes/versions?count=${showCount}`, function (data) {
            const length = data.data.length;

            if (length > 0) {

                const keys = data.data.map(x => {
                    return x.key;
                });
                const values = data.data.map(x => {
                    return x.value;
                });

                var element = document.getElementById('versions');
                if (update) {
                    updateVersionChart(element, keys, values);
                } else {
                    initVersionChart(element, keys, values);
                }

            }
        });
    }

    var initNodeChart = function (element, keys, values) {

        if (!element) {
            return;
        }

        var parent = $('#node-time-stats');
        var width = parent.width();

        var height = parseInt(KTUtil.css(element, 'height'));
        var borderColor = KTUtil.getCssVariableValue('--bs-border-dashed-color');
        var baseColor = KTUtil.getCssVariableValue('--bs-gray-800');
        var lightColor = KTUtil.getCssVariableValue('--bs-light-info');

        var options = {
            series: [{
                name: 'Nodes',
                data: values
            }],
            chart: {
                fontFamily: 'inherit',
                type: 'area',
                height: height,
                width: width,
                toolbar: {
                    show: false
                }
            },
            legend: {
                show: false
            },
            dataLabels: {
                enabled: false
            },
            fill: {
                type: 'solid',
                opacity: 0
            },
            stroke: {
                curve: 'smooth',
                show: true,
                width: 2,
                colors: [baseColor]
            },
            xaxis: {
                axisBorder: {
                    show: false
                },
                axisTicks: {
                    show: false
                },
                labels: {
                    show: false
                },
                crosshairs: {
                    position: 'front',
                    stroke: {
                        color: baseColor,
                        width: 1,
                        dashArray: 3
                    }
                },
                tooltip: {
                    enabled: true,
                    formatter: undefined,
                    offsetY: 0,
                    style: {
                        fontSize: '12px'
                    }
                }
            },
            yaxis: {
                labels: {
                    show: false
                }
            },
            states: {
                normal: {
                    filter: {
                        type: 'none',
                        value: 0
                    }
                },
                hover: {
                    filter: {
                        type: 'none',
                        value: 0
                    }
                },
                active: {
                    allowMultipleDataPointsSelection: false,
                    filter: {
                        type: 'none',
                        value: 0
                    }
                }
            },
            tooltip: {
                style: {
                    fontSize: '12px'
                },
                x: {
                    formatter: function (val) {
                        return keys[val - 1];
                    }
                },
                y: {
                    formatter: function (val) {
                        return val;
                    }
                }
            },
            colors: [lightColor],
            grid: {
                strokeDashArray: 4,
                padding: {
                    top: 0,
                    right: -20,
                    bottom: -20,
                    left: -20
                },
                yaxis: {
                    lines: {
                        show: true
                    }
                }
            },
            markers: {
                strokeColor: baseColor,
                strokeWidth: 2
            }
        };

        var chart = new ApexCharts(element, options);

        // Set timeout to properly get the parent elements width
        setTimeout(function () {
            chart.render();
        }, 300);
    }

    var initVersionChart = function (element, keys, values) {
        if (!element) {
            return;
        }

        const options = createVersionOptions(element, keys, values);

        var chart = new ApexCharts(element, options);

        // Set timeout to properly get the parent elements width
        setTimeout(function () {
            chart.render();
        }, 300);
    }

    var updateVersionChart = function (element, keys, values) {

        const options = createVersionOptions(element, keys, values);

        ApexCharts.exec('versionChart', 'updateOptions', options, false, true);
    }

    var createVersionOptions = function (element, keys, values) {
        var parent = $('#version-card');
        var width = parent.width();

        var height = parseInt(KTUtil.css(element, 'height'));
        var borderColor = KTUtil.getCssVariableValue('--bs-border-dashed-color');
        var baseColor = KTUtil.getCssVariableValue('--bs-gray-800');
        var lightColor = KTUtil.getCssVariableValue('--bs-light-info');


        var options = {
            series: values,
            chart: {
                height: height,
                width: width,
                id: 'versionChart',
                type: 'pie'
            },
            plotOptions: {
                pie: {
                    startAngle: -90,
                    endAngle: 270
                }
            },
            dataLabels: {
                enabled: false
            },
            fill: {
                type: 'gradient'
            },
            labels: keys,
            legend: {
                formatter: function (val, opts) {
                    return val + ' - ' + opts.w.globals.series[opts.seriesIndex];
                }
            },
            title: {
                text: ''
            },
            responsive: [{
                breakpoint: 480,
                options: {
                    chart: {
                        width: 200
                    },
                    legend: {
                        position: 'bottom'
                    }
                }
            }]
        };

        return options;
    }

    var sumArray = function (arrayVals) {
        let sum = 0;

        for (let i = 0; i < arrayVals.length; i++) {
            sum += arrayVals[i];
        }

        return sum;
    }

    var updateVerifications = function () {
        $.get('/api/nodes/verifying', function (data) {
            const length = data.data.length;

            if (length > 0) {
                const values = data.data.map(x => {
                    return x.value;
                });

                const topValue = data.data[0].value;
                const percentage = (topValue * 100) / sumArray(values);

                const element = document.getElementById('verification-display');
                element.innerText = `${percentage}%`;

                const ruler = document.getElementById('verification-ruler');
                ruler.style.width = `${percentage}%`;
            }
        });
    }

    var updateStateType = function () {
        $.get('/api/nodes/state-types', function (data) {
            const length = data.data.length;

            if (length > 0) {
                const values = data.data.map(x => {
                    return x.value;
                });

                const topValue = data.data[0].value;
                const percentage = (topValue * 100) / sumArray(values);

                const element = document.getElementById('state-display');
                element.innerText = `${percentage}%`;

                const ruler = document.getElementById('state-ruler');
                ruler.style.width = `${percentage}%`;
            }
        });
    }

    var updateBlocks = function () {
        $.get('/api/nodes/blocks-kept', function (data) {
            const length = data.data.length;

            if (length > 0) {
                const values = data.data.map(x => {
                    return x.value;
                });

                const topValue = data.data[0].value;
                const percentage = (topValue * 100) / sumArray(values);

                const element = document.getElementById('blocks-display');
                element.innerText = `${percentage}%`;

                const ruler = document.getElementById('blocks-ruler');
                ruler.style.width = `${percentage}%`;
            }
        });
    }

    $(document).ready(function () {
        updateDailyNodeCount();
        updateWeeklyNodeCount();
        updateMonthlyNodeCount();
        getVersions(5, false);
        updateVerifications();
        updateStateType();
        updateBlocks();

        $('.country-chooser').click(function () {
            root.container.children.clear();

            var elem = this;
            var count = parseInt(elem.innerText);
            updateMap(count);
            var display = document.getElementById('country-display');
            display.innerText = `Top ${count} countries`;
        });

        $('.version-chooser').click(function () {

            var elem = this;
            var count = parseInt(elem.innerText);
            getVersions(count, true);

            var display = document.getElementById('version-display');
            display.innerText = `Top ${count} versions`;
        });

    });

    var root = null;

    am5.ready(function () {
        root = am5.Root.new("node-map");
        updateMap(5);

    });

})();