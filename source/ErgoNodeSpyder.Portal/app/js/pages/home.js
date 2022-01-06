$(document).ready(function () {
    updateDailyNodeCount();
    updateWeeklyNodeCount();
    updateMonthlyNodeCount();
});

var updateDailyNodeCount = function() {
    $.get('/api/nodes/daily-count', function (data) {
        const length = data.data.length;
        console.log('daily values received');
        if (length > 0) {
            const val = data.data[length - 1];
            $('#daily-node-count').text(val.value);
            const keys = data.data.map(x => {
                return x.key;
            });
            const values = data.data.map(x => {
                return x.value;
            });
            initDailyChart(keys, values);
        }
    });
}

var updateWeeklyNodeCount = function () {
    $.get('/api/nodes/weekly-count', function (data) {
        const length = data.data.length;
        console.log('weekly values received');
        if (length > 0) {
            const val = data.data[length - 1];
            $('#weekly-node-count').text(val.value);
            const keys = data.data.map(x => {
                return x.key;
            });
            const values = data.data.map(x => {
                return x.value;
            });
            initWeeklyChart(keys, values);
        }
    });
}

var updateMonthlyNodeCount = function () {
    $.get('/api/nodes/monthly-count', function (data) {
        const length = data.data.length;
        console.log('monthly values received');
        if (length > 0) {
            const val = data.data[length - 1];
            $('#monthly-node-count').text(val.value);
            const keys = data.data.map(x => {
                return x.key;
            });
            const values = data.data.map(x => {
                return x.value;
            });
            initMonthlyChart(keys, values);
        }
    });
}

var initDailyChart = function (keys, values) {
    var element = document.getElementById('daily-node-display');

    if (!element) {
        return;
    }

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
                    return keys[val-1];
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

var initWeeklyChart = function (keys, values) {
    var element = document.getElementById('weekly-node-display');

    if (!element) {
        return;
    }

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

var initMonthlyChart = function (keys, values) {
    var element = document.getElementById('monthly-node-display');

    if (!element) {
        return;
    }

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