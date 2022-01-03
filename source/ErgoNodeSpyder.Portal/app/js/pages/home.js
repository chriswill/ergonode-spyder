$(document).ready(function () {
    updateNodeCount();
});

function updateNodeCount() {
    $.get('/api/nodes/daily-count', function (data) {
        debugger;
        const length = data.data.length;
        if (length > 0) {
            const val = data.data[length - 1];
            $('#node-count').text(val.value);
        }
    });
}