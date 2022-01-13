(function () {

    var updateContinents = function () {
        $.get('/api/nodes/geo/continents', function (data) {
            const length = data.data.length;

            if (length > 0) {
                
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

    var updateCountries = function(count) {
        $.get(`/api/nodes/geo/countries?count=${count}`, function(data) {
            const length = data.data.length;

            if (length > 0) {

                var element = document.getElementById("countries-widget");

                if (!element) {
                    return;
                }

                let docFragment = document.createDocumentFragment();

                for (var i = 0; i < data.data.length; i++) {
                    var item = data.data[i];

                    let tr = document.createElement('tr');
                    let td1 = document.createElement('td');
                    let countryImg = document.createElement('img');
                    countryImg.setAttribute('src', `/images/flags/${item.code}.svg`);
                    countryImg.setAttribute('class', 'me-4 w-25px');
                    countryImg.setAttribute('style', 'border-radius: 4px');
                    countryImg.setAttribute('alt', '');
                    td1.appendChild(countryImg);
                    tr.appendChild(td1);
                    let td2 = document.createElement('td');
                    td2.setAttribute('class', 'ps-0');
                    let a = document.createElement('a');
                    a.setAttribute('href', `/geo/country/${item.code}`);
                    a.setAttribute('title', 'View details');
                    a.setAttribute('class', 'text-gray-800 fw-bolder text-hover-primary mb-1 fs-6 text-start pe-0');
                    a.appendChild(document.createTextNode(item.name));
                    td2.appendChild(a);
                    tr.appendChild(td2);
                    let td3 = document.createElement('td');
                    let span = document.createElement('span');
                    span.setAttribute('class', 'text-gray-800 fw-bolder d-block fs-6 ps-0 text-end');
                    span.appendChild(document.createTextNode(item.value));
                    td3.appendChild(span);
                    tr.appendChild(td3);

                    docFragment.appendChild(tr);
                }

                while (element.firstChild) {
                    element.removeChild(element.firstChild);
                }

                element.appendChild(docFragment);
            }
        });
    }
    
    var updateIsps = function(count) {
        $.get(`/api/nodes/geo/isps?count=${count}`, function(data) {
            const length = data.data.length;

            if (length > 0) {

                var element = document.getElementById('isp-widget');

                if (!element) {
                    return;
                }

                let docFragment = document.createDocumentFragment();

                for (var i = 0; i < data.data.length; i++) {
                    var item = data.data[i];

                    let tr = document.createElement('tr');
                    let td1 = document.createElement('td');
                    td1.setAttribute('class', 'ps-0');
                    let nameSpan = document.createElement('span');
                    nameSpan.setAttribute('class',
                        'text-gray-800 fw-bolder mb-1 fs-6 text-start pe-0');
                    nameSpan.appendChild(document.createTextNode(item.key));
                    td1.appendChild(nameSpan);
                    tr.appendChild(td1);
                    let td2 = document.createElement('td');
                    let span = document.createElement('span');
                    span.setAttribute('class', 'text-gray-800 fw-bolder d-block fs-6 ps-0 text-end');
                    span.appendChild(document.createTextNode(item.value));
                    td2.appendChild(span);
                    tr.appendChild(td2);

                    docFragment.appendChild(tr);
                }

                while (element.firstChild) {
                    element.removeChild(element.firstChild);
                }

                element.appendChild(docFragment);
            }
        });
    }

    $(document).ready(function () {
        updateContinents();
        updateCountries(10);
        updateIsps(10);

        $('.country-chooser').click(function () {
            
            var button = this;

            var countryCount;
            if (button.innerText !== 'All') {
                countryCount = parseInt(button.innerText);
            } else {
                countryCount = 1000;
            }
            
            updateCountries(countryCount);
            var display = document.getElementById('country-display');
            if (button.innerText !== 'All') {
                display.innerText = `Top ${countryCount} countries`;
            } else {
                display.innerText = 'All countries';
            }
            
        });

        $('.isp-chooser').click(function () {

            var button = this;

            var ispCount;
            if (button.innerText !== 'All') {
                ispCount = parseInt(button.innerText);
            } else {
                ispCount = 1000;
            }

            updateIsps(ispCount);
            var display = document.getElementById('isp-display');
            if (button.innerText !== 'All') {
                display.innerText = `Top ${ispCount} ISPs`;
            } else {
                display.innerText = 'All ISPs';
            }

        });
    });

})();