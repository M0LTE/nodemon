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
connection.on("ReceiveMessage", function (message) {
    return
    /*var li = document.createElement("li");
    var list = document.getElementById("messagesList")
    list.appendChild(li);

    if (list.childNodes.length > 1000) {
        list.removeChild(list.firstChild)
    }

    li.innerHTML = `<pre>${message.timestamp} ${message.modemId} ${htmlEncode(message.data)}</pre>`;
    if (message.direction == '>') {
        li.style.color = 'red'
    } else if (message.direction == '<') {
        if (message.data.includes(' > GB7RDG')) { // sorry
            li.style.color = 'green'
        } else {
            li.style.color = 'gray'
        }
    }

    $("html, body").animate({ scrollTop: $(document).height() }, 50);*/
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});
