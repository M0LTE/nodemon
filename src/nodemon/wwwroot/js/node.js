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
    //document.getElementById("rssi").innerText = rssi

    lineChart.data.series[0].push(rssi);
    if (lineChart.data.series[0].length > points) {
        lineChart.data.series[0].shift()
    }
    lineChart.update();
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});

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
});