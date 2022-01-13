"use strict";
var sumArray = function (arrayVals) {
    let sum = 0;

    for (let i = 0; i < arrayVals.length; i++) {
        sum += arrayVals[i].value;
    }

    return sum;
}

var getIsps = function (countryCode) {
    $.get(`/api/nodes/geo/isps?countryCode=${countryCode}&count=1000`, function (data) {
        var length = data.data.length;
        if (length > 0) {

            var total = sumArray(data.data);
            var elem = document.getElementById('country-node-count');
            elem.innerText = total;

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

var getRegions = function(countryCode) {
    $.get(`/api/nodes/geo/country-info/${countryCode}`, function (data) {
        var length = data.data.length;
        if (length > 0) {
            var element = document.getElementById('location-widget');

            if (!element) {
                return;
            }

            let docFragment = document.createDocumentFragment();

            for (var i = 0; i < data.data.length; i++) {
                var item = data.data[i];

                let tr = document.createElement('tr');
                let td1 = document.createElement('td');
                td1.setAttribute('class', 'ps-0');
                let regionSpan = document.createElement('span');
                regionSpan.setAttribute('class',
                    'text-gray-800 fw-bolder mb-1 fs-6 text-start pe-0');
                regionSpan.appendChild(document.createTextNode(item.region));
                td1.appendChild(regionSpan);
                tr.appendChild(td1);

                let td2 = document.createElement('td');
                let citySpan = document.createElement('span');
                citySpan.appendChild(document.createTextNode(item.city));
                td2.appendChild(citySpan);
                tr.appendChild(td2);
                let td3 = document.createElement('td');
                let span = document.createElement('span');
                span.setAttribute('class', 'text-gray-800 fw-bolder d-block fs-6 ps-0 text-end');
                span.appendChild(document.createTextNode(item.count));
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