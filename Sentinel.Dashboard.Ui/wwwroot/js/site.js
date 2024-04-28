function miniChartFormatter(params) {
    if (Array.isArray(params))
    {
        output = '<div style="font-size:14px;color:#666;font-weight:400;line-height:1;">' + params[0].name + '</div>'
        output += '<div style="margin-top: 5px">'

        for (i = 0; i < params.length; i++) {
            output += '<div>' + params[i].marker + params[i].seriesName + ': <span style="float:right;margin-left:20px;color:#666;font-weight:900">' + params[i].value + '</span></div>'
        }

        output += '</div>'
        return output
    } else if (params.componentType === 'markPoint')
    {
        output = '<div style="font-size:14px;color:#666;font-weight:400;line-height:1;">' + params.name + '</div>'
        output += '<div style="margin-top: 5px">'
        output += '<div>' + params.value + '</div>'
        output += '</div>'
        return output
    }

    output = '<div style="font-size:14px;color:#666;font-weight:400;line-height:1;">' + params.name + '</div>'
    output += '<div style="margin-top: 5px">'
    output += '<div>' + params.marker + params.seriesName + ': <span style="float:right;margin-left:20px;color:#666;font-weight:900">' + params.value + '</span></div>'
    output += '</div>'
    return output
}

function sortByKey(array, key) {
    if (array.length == 0) return [];
    
    return array.sort(function(a, b) {
        var x = a[key]; var y = b[key];
        return ((x < y) ? -1 : ((x > y) ? 1 : 0));
    });
}

function overviewChartFormatter(params) {
    output = '<div>' + params[0].name
    events = params.reduce((a, b) => +a + +b.value, 0)
    if (events > 0) {
        output += '<span style="float:right;margin-left:20px;color:#666;font-weight:900">Events</span></div>'
        output += '<div style="margin-top: 5px">'
        for (i = 0; i < params.length; i++) {
            if (params[i].value > 0){
                output += '<div>' + params[i].marker + params[i].seriesName + ': <span style="float:right;margin-left:20px;color:#666;font-weight:900">' + params[i].value + '</span></div>';
            }
        }
    }
    output += '</div>'
    return output
}

function buildOverviewChart(category, data){
    options = {
        tooltip: {
            trigger: 'axis',
            confine: true,
            axisPointer: {
                type: 'shadow'
            },
            formatter: overviewChartFormatter
        },
        legend: {
            show: false,
        },
        xAxis: {
            data: category
        },
        grid: {
            left: 10,
            containLabel: true,
            bottom: 0,
            top: 20,
            right: 10
        },
        yAxis: {
            type: 'value',
            splitLine: {
                show: false
            },
            scale: true
        },
        animationDuration: 300,
        series: data
    };
    
    return options;
}

function buildOverviewMiniChart(category, data){
    options = {
        tooltip: {
            trigger: 'axis',
            axisPointer: {
                type: 'shadow'
            },
            formatter: miniChartFormatter
        },
        grid: {
            left: 30,
            containLabel: false,
            bottom: 2,
            top: 5,
            right: 0
        },
        xAxis: {
            type: 'category',
            data: category,
            show: false,
        },
        yAxis: {
            type: 'value',
            show: false,
            scale: true
        },
        animationDuration: 150,
        series: [
            {
                name: 'Events',
                data: data,
                type: 'bar',
                itemStyle: {
                    color: '#ffcf7d'
                },
                barMinHeight: 1,
                barWidth: '85%',
                markLine: {
                    symbol: [],
                    data: [
                        {
                            label: {
                                position: 'start'
                            },
                            lineStyle: {
                                type: 'solid',
                                opacity: 0.5
                            },
                            type: 'max'
                        }
                    ],
                    animation: false
                }
            }
        ]
    };
    
    return options;
}

function formatBytes(bytes, decimals = 2) {
    if (bytes === 0) return '0 Bytes';

    const k = 1024;
    const dm = decimals < 0 ? 0 : decimals;
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];

    const i = Math.floor(Math.log(bytes) / Math.log(k));

    return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
}

function buildDetailMiniChart(json){
    options = {
        tooltip: {
            trigger: 'item',
            formatter: miniChartFormatter
        },
        grid: {
            left: 4,
            bottom: 4,
            top: 0,
            right: 4
        },
        xAxis: {
            type: 'category',
            data: json.categories,
            axisPointer: {
                show: true,
                label: {
                    show: false
                },
                type: 'shadow'
            },
            show: false,
        },
        yAxis: {
            type: 'value',
            show: false,
            scale: true
        },
        animationDuration: 150,
        series: [{
            name: 'Events',
            data: json.series,
            type: 'bar',
            itemStyle: {
                color: '#ffcf7d'
            },
            barMinHeight: 1,
            showBackground: false,
            barWidth: '85%'
        }
        ]
    };

    var markPoint = {
        data: [],
        label: {
            show: false
        },
        symbol: "circle",
        symbolSize: 7,
    }

    if (json.firstSeenIndex > -1){
        let first = {
            name: 'First seen',
            value: moment(json.issue.firstSeen).format('MMMM Do, HH:mm'),
            xAxis: json.firstSeenIndex,
            yAxis: 0,
            itemStyle: {
                color: 'red'
            },
            symbolOffset: [-4, 0]
        }
        markPoint.data.push(first)
    }

    if (json.lastSeenIndex > -1){
        let last = {
            name: 'Last seen',
            value: moment(json.issue.lastSeen).format('MMMM Do, HH:mm'),
            xAxis: json.lastSeenIndex,
            yAxis: 0,
            itemStyle: {
                color: 'green'
            },
            symbolOffset: [4, 0]
        }
        markPoint.data.push(last)
    }
    
    if (markPoint.data.length > 0){
        options.series[0]["markPoint"] = markPoint;
    }
    
    return options;
}