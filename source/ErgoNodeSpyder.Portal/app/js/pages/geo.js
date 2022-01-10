(function () {

    var updateContinents = function () {
        $.get('/api/nodes/geo/continents', function (data) {
            const length = data.data.length;

            if (length > 0) {
                debugger;
                const keys = data.data.map(x => {
                    return x.name;
                });
                const values = data.data.map(x => {
                    return x.value;
                });

                var element = document.getElementById("continents-widget");

                if (!element) {
                    return;
                }

                var borderColor = KTUtil.getCssVariableValue('--bs-border-dashed-color');

                var options = {
                    series: [{
                        name: 'Nodes',
                        data: values,
                        show: true
                    }],
                    chart: {
                        type: 'bar',
                        height: 250,
                        toolbar: {
                            show: false
                        }
                    },
                    plotOptions: {
                        bar: {
                            borderRadius: 4,
                            horizontal: true,
                            distributed: true,
                            barHeight: 23
                        }
                    },
                    dataLabels: {
                        enabled: false
                    },
                    legend: {
                        show: false
                    },
                    colors: ['#3E97FF', '#F1416C', '#50CD89', '#FFC700', '#7239EA', '#50CDCD', '#3F4254'],
                    xaxis: {
                        categories: keys,
                        labels: {
                            formatter: function (val) {
                                return val;
                            },
                            style: {
                                colors: KTUtil.getCssVariableValue('--bs-gray-400'),
                                fontSize: '14px',
                                fontWeight: '600',
                                align: 'left'
                            }
                        },
                        axisBorder: {
                            show: false
                        }
                    },
                    yaxis: {
                        labels: {
                            style: {
                                colors: KTUtil.getCssVariableValue('--bs-gray-800'),
                                fontSize: '14px',
                                fontWeight: '600'
                            },
                            offsetY: 2,
                            align: 'left'
                        }
                    },
                    grid: {
                        borderColor: borderColor,
                        xaxis: {
                            lines: {
                                show: true
                            }
                        },
                        yaxis: {
                            lines: {
                                show: false
                            }
                        },
                        strokeDashArray: 4
                    }
                };

                var chart = new ApexCharts(element, options);

                setTimeout(function () {
                    chart.render();
                }, 300);
            }
        });
    }

    var updateCountries = function () {
        $.get('/api/nodes/geo/countries?count=25', function (data) {
            const length = data.data.length;

            if (length > 0) {
                debugger;
                const keys = data.data.map(x => {
                    return x.name;
                });
                const values = data.data.map(x => {
                    return x.value;
                });

                var element = document.getElementById("countries-widget");

                if (!element) {
                    return;
                }

                var borderColor = KTUtil.getCssVariableValue('--bs-border-dashed-color');

                var options = {
                    series: [{
                        name: 'Nodes',
                        data: values,
                        show: true
                    }],
                    chart: {
                        type: 'bar',
                        height: 600,
                        toolbar: {
                            show: false
                        }
                    },
                    plotOptions: {
                        bar: {
                            borderRadius: 4,
                            horizontal: true,
                            distributed: true,
                            barHeight: 23
                        }
                    },
                    dataLabels: {
                        enabled: false
                    },
                    legend: {
                        show: false
                    },
                    colors: ['#3E97FF', '#F1416C', '#50CD89', '#FFC700', '#7239EA', '#50CDCD', '#3F4254'],
                    xaxis: {
                        categories: keys,
                        labels: {
                            formatter: function (val) {
                                return val;
                            },
                            style: {
                                colors: KTUtil.getCssVariableValue('--bs-gray-400'),
                                fontSize: '14px',
                                fontWeight: '600',
                                align: 'left'
                            }
                        },
                        axisBorder: {
                            show: false
                        }
                    },
                    yaxis: {
                        labels: {
                            style: {
                                colors: KTUtil.getCssVariableValue('--bs-gray-800'),
                                fontSize: '14px',
                                fontWeight: '600'
                            },
                            offsetY: 2,
                            align: 'left'
                        }
                    },
                    grid: {
                        borderColor: borderColor,
                        xaxis: {
                            lines: {
                                show: true
                            }
                        },
                        yaxis: {
                            lines: {
                                show: false
                            }
                        },
                        strokeDashArray: 4
                    }
                };

                var chart = new ApexCharts(element, options);

                setTimeout(function () {
                    chart.render();
                }, 300);
            }
        });
    }

    $(document).ready(function () {
        updateContinents();
        updateCountries();
    });

})();