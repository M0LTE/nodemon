"use strict";

// https://codepen.io/k3no/pen/jMNGBR

var lineChart = new Chartist.Line('#chartArea', {
    labels: [],
    series: [[]]
},
{
    low: -140,
    high: -30,
    showArea: true,
    axisX: {
        showGrid: false
    },
    showPoint: false,
    height: 400,
    lineSmooth: true,
    lineWidth: 1
});

var connection = new signalR.HubConnectionBuilder()
    .withUrl("/nodeHub")
    .withAutomaticReconnect()
    .build();

const points = 250;

for (var i = 0; i < points; i++) {
    lineChart.data.series[0].push(-150);
}

connection.on("RssiUpdate", function (rssi) {
    if (rssi.p == "2m") {
        lineChart.data.series[0].push(rssi.r);
        if (lineChart.data.series[0].length > points) {
            lineChart.data.series[0].shift()
        }
        lineChart.update();
    }
});

connection.on("VswrUpdate", function (vswr) {
    if (vswr.p == "2m") {
        $('#vswr_1').text(vswr.v);
    }
});

connection.on("MonitorHeard", function (mon) {
    var tb = $('#monitor')
    tb.val(tb.val() + "\n" + mon.port + ": " + mon.data)
})

connection.start().then(function () {
    connection.invoke("GetChannel", "2m").then(function (result) {
        $('#channel_1').val(result);
    });
}).catch(function (err) {
    return console.error(err.toString());
})

$(document).ready(function () {
    $('#toggle1').change(function () {
        connection.invoke("ToggleChanged", 7, this.checked).catch(function (err) {
            return console.error(err.toString());
        })
    });

    $('#toggle2').change(function () {
        connection.invoke("ToggleChanged", 6, this.checked).catch(function (err) {
            return console.error(err.toString());
        })
    });

    $('#channel_1').on('change', function () {
        connection.invoke("ChannelChanged", "2m", this.value).catch(function (err) {
            return console.error(err.toString());
        })
    });

    $('#ninoMode_1').on('change', function () {
        fetch('/api/ports/2m/modem/mode?id=' + this.value, {
            method: 'put'
        }).then(() => {
            console.log('posted ' + this.value)
        })
    });

    //fetch('/api/ports/2m/modem/mode')
        //.then(response => {
        //});
    //$('ninoMode_1')
});