<dummy>
</dummy>

<main-tab>
<ul class="tab">
    <li each={name, i in mainTabs} class={select: mainTab === i} onclick={parent.changeTab}>{name}</li>
</ul>

<script>
/* global moment, c3, opts */

this.mainTab = +sessionStorage.getItem('prevTab');
opts.observable.trigger("mainTabChanged", this.mainTab);

this.changeTab = function(e) {
    this.mainTab = e.item.i;
    sessionStorage.setItem('prevTab', e.item.i);
    opts.observable.trigger("mainTabChanged", e.item.i);
}.bind(this);
</script>
</main-tab>

<log-term>
<div id="term" show={enabled}>
<ul class="tab tabsub" style="float: left; margin-right: 0.2em">
    <li each={name, i in rangeTabs} class={select: opts.logRange.val === i} onclick={parent.rangeTabChange}>{name}</li>
</ul>
<div style="padding: 0.2em 0;">
<input type="text" id="term_from" style="width: 10em">～<input type="text" id="term_to" style="width: 10em">
</div>
</div>

<script>
var self = this;

this.rangeTabs = [
"今日",
"今週",
"今月",
"すべて",
"期間指定"
];

this.enabled = false;

opts.observable.on("mainTabChanged", function(idx) {
    self.update({enabled: idx >= 0 && idx < self.logTables});
});

var val = sessionStorage.getItem('logRange');
opts.logRange.val = val === null ? 2 : +val;

this.init = function() {
    $('#term_from').datetimepicker({
        onClose: function() {
            if (opts.logRange.val === 4)
                opts.observable.trigger("logRangeChanged");
        }
    });
    $('#term_to').datetimepicker({
        onClose: function() {
            if (opts.logRange.val === 4)
                opts.observable.trigger("logRangeChanged");
        }
    });
};

this.on("mount", this.init);

this.rangeTabChange = function(e) {
    sessionStorage.setItem("logRange", e.item.i);
    opts.logRange.val = e.item.i;
    opts.observable.trigger("logRangeChanged");
};

</script>
</log-term>

<log-tables>
<div each={header, i in tables} show={mainTab === i}>
<table class="display compact cell-border" id={"log" + i}>
<thead>
<tr></tr>
</thead>
</table>
</div>

<script>
this.tables = [
"<th>日付</th><th>海域</th><th>マップ</th><th>マス</th><th>ボス</th><th>ランク</th><th>ドロップ艦種</th><th>ドロップ艦娘", // ドロップ
"<th>日付</th><th style=\"min-width: 3.2em;\">海域</th><th>マップ</th><th>マス</th><th>ボス</th><th>ランク</th><th>艦隊行動</th><th>味方陣形</th><th>敵陣形</th><th style=\"min-width: 3.2em;\">敵艦隊</th><th>味方艦1</th><th>味方艦1HP</th><th>味方艦2</th><th>味方艦2HP</th><th>味方艦3</th><th>味方艦3HP</th><th>味方艦4</th><th>味方艦4HP</th><th>味方艦5</th><th>味方艦5HP</th><th>味方艦6</th><th>味方艦6HP</th><th>大破艦</ht><th style=\"min-width: 2.2em;\">敵艦1</th><th>敵艦1HP</th><th style=\"min-width: 2.2em;\">敵艦2</th><th>敵艦2HP</th><th style=\"min-width: 2.2em;\">敵艦3</th><th>敵艦3HP</th><th style=\"min-width: 2.2em;\">敵艦4</th><th>敵艦4HP</th><th style=\"min-width: 2.2em;\">敵艦5</th><th>敵艦5HP</th><th style=\"min-width: 2.2em;\">敵艦6</th><th>敵艦6HP</th><th>味方制空値</th><th>敵制空値</th><th>制空状態</th>", // 海戦
"<th>日付</th><th>結果</th><th>遠征</th><th>燃料</th><th>弾薬</th><th>鋼材</th><th>ボーキ</th><th>開発資材</th><th>高速修復材</th><th>高速建造材</th>", // 遠征
"<th>日付</th><th>開発装備</th><th>種別</th><th>燃料</th><th>弾薬</th><th>鋼材</th><th>ボーキ</th><th>秘書艦</th><th>司令部Lv</th>", // 開発
"<th>日付</th><th>種類</th><th>名前</th><th>艦種</th><th>燃料</th><th>弾薬</th><th>鋼材</th><th>ボーキ</th><th>開発資材</th><th>空きドック</th><th>秘書艦</th><th>司令部Lv</th>", // 建造
"<th>日付</th><th>改修装備</th><th>レベル</th><th>成功</th><th>確実化</th><th>消費装備</th><th>消費数</th><th>燃料</th><th>弾薬</th><th>鋼材</th><th>ボーキ</th><th>開発資材</th><th>改修資材</th><th>秘書艦</th><th>二番艦</th>", // 改修
"<th>日付</th><th>燃料</th><th>弾薬</th><th>鋼材</th><th>ボーキ</th><th>高速建造材</th><th>高速修復材</th><th>開発資材</th><th>改修資材</th>" // 戦果
];

this.jsons = [
    "海戦・ドロップ報告書.json",
    "海戦・ドロップ報告書.json",
    "遠征報告書.json",
    "開発報告書.json",
    "建造報告書.json",
    "改修報告書.json",
    "資材ログ.json"
];

this.on("mount", function() {
    var records = this.root.querySelectorAll("tr");
    for (var i = 0; i < records.length; i++)
        records[i].innerHTML = this.tables[i];
    this.init();
});

this.mainTab = 0;
var self = this;

opts.observable.on("mainTabChanged", function(idx) {
    self.update({mainTab: idx});
    self.show();
});

opts.observable.on("logRangeChanged", function() {
    self.show();
});

this.init = function() {
    for (var t = 0; t < this.tables.length; t++) {
        var opts = {
            destroy: true,
            deferRender: true,
            stateSave: true,
            order: [[0, "desc"]],
            pageLength: 50,
            lengthMenu: [[50, 100, 200, -1], [50, 100, 200, "All"]],
            drawCallback: function() {
                $('#loading').hide();
            }
        };
        if (t === 0) {
            opts.columns = [{data: 0}, {data: 1}, {data: 39}, {data: 2}, {data: 3}, {data: 4}, {data: 9}, {data: 10}];
        } else if (t === 1) {
            var entries = [];
            for (var i = 0; i < 38; i++) {
                if (i === 2)
                    entries.push({data: 39});
                if (i === 9 || i === 10)
                    continue;
                if (i === 23)
                    entries.push({data: 38});
                entries.push({data: i});
            }
            opts.columns = entries;
        }
        $('#log' + t).dataTable(opts);
    }
};

this.show = function() {
    if (this.mainTab >= this.jsons.length)
        return;
    var now = moment();
    var from;
    var query = "?from=";
    switch (opts.logRange.val){
    case 0:
        from = now.clone().startOf('day').hours(5);
        if (now.hour() < 5)
            from.subtract(1, 'days');
        query += from.valueOf();
        break;
    case 1:
        from = now.clone().startOf('week').hours(5);
        if (now.hour() < 5 && now.days() === 1)
            from.subtract(1, 'weeks');
        query += from.valueOf();
        break;
    case 2:
        if (now.hours() >= 22 &&
            now.dates() === now.clone().endOf('month').date()) {
            from = now.clone().hours(22);
        } else {
            from = now.clone().startOf('month').subtract(1, 'days').hours(22);
        }
        query += from.valueOf();
        break;
    case 3:
        query = "";
        break;
    case 4:
        from = $('#term_from').datetimepicker("getValue");
        var to = $('#term_to').datetimepicker("getValue");
        if (from === null)
            return;
        query += from.valueOf();
        if (to !== null)
            query += "&to=" + to.valueOf();
        break;
    }
    $('#loading').show();
    var url = this.jsons[this.mainTab] + query;
    $('#log' + this.mainTab).DataTable().ajax.url(url).load();
};
</script>
</log-tables>

<chart-type>
<form id="chart_type" show={mainTabs[mainTab] === "資材グラフ"}>
<div style="margin: 0 0 0.5em 1em;">
<label><input type="radio" name="chart_type" value="0" checked={opts.chartSpec.type === 0} onchange={chartTypeChange}>連続</label>
<label><input type="radio" name="chart_type" value="1" checked={opts.chartSpec.type === 1} onchange={chartTypeChange}>差分</label>
</div>
</form>

<script>
this.mainTab = 0;
opts.chartSpec.type = +sessionStorage.getItem('chartType');
var self = this;

this.chartTypeChange = function(e) {
    opts.chartSpec.type = +e.target.value;
    sessionStorage.setItem('chartType', opts.chartSpec.type);
    opts.observable.trigger("chartTypeChanged");
    opts.observable.trigger("chartSpecChanged");
};

opts.observable.on("mainTabChanged", function(idx) {
    self.update({mainTab: idx});
});
</script>
</chart-type>

<chart-range>
<div show={mainTabs[mainTab] === "資材グラフ"}>
<ul class="tab tabsub" style="float: left; margin-right: 0.2em" show={chartSpec.type === 0}>
    <li each={name, i in seqChartRanges} class={select: chartSpec.seqRange === i} onclick={parent.rangeTabChange}>{name}</li>
</ul>

<ul class="tab tabsub" style="float: left; margin-right: 0.2em" show={chartSpec.type === 1}>
    <li each={name, i in diffChartRanges} class={select: chartSpec.diffRange === i} onclick={parent.rangeTabChange}>{name}</li>
</ul>
<div style="padding: 0.2em 0;">
<input type="text" id="chart_from" style="width: 10em">～<input type="text" id="chart_to" style="width: 10em">
<label><input type="checkbox" id="tooltip" value="" style="margin-left: 2em;" onchange={tooltipChange} checked={opts.chartSpec.tooltip === 1}>ツールチップ</label>
</div>
</div>

<script>
this.seqChartRanges = [
"一日",
"一週間",
"一か月",
"三か月",
"すべて",
"期間指定"
];

this.diffChartRanges = [
"一か月(日)",
"三か月(日)",
"半年(週)",
"すべて(月)",
"期間指定"
];

opts.chartSpec.seqRange = +sessionStorage.getItem('seqChartRange');
opts.chartSpec.diffRange = +sessionStorage.getItem('diffChartRange');
opts.chartSpec.tooltip = +sessionStorage.getItem('chartTooltip');
this.chartSpec = opts.chartSpec;

this.rangeTabChange = function(e) {
    if (opts.chartSpec.type === 0) {
        opts.chartSpec.seqRange = e.item.i;
        sessionStorage.setItem('seqChartRange', e.item.i);
    } else {
        opts.chartSpec.diffRange = e.item.i;
        sessionStorage.setItem('diffChartRange', e.item.i);
    }
    opts.observable.trigger("chartSpecChanged");
};

this.tooltipChange = function(e) {
    opts.chartSpec.tooltip = +e.target.checked;
    sessionStorage.setItem('chartTooltip', +e.target.checked);
    opts.observable.trigger("chartSpecChanged");
};

this.useDatePicker = function() {
    return opts.chartSpec.type === 0 && opts.chartSpec.seqRange === 5 ||
        opts.chartSpec.type === 1 && opts.chartSpec.diffRange === 4;
};

this.init = function() {
    $('#chart_from').datetimepicker({
        onClose: function() {
            if (self.useDatePicker())
                opts.observable.trigger("chartSpecChanged");
        }
    });
    $('#chart_to').datetimepicker({
        onClose: function() {
            if (self.useDatePicker())
                opts.observable.trigger("chartSpecChanged");
        }
    });
};

this.mainTab = 0;
var self = this;

this.on("mount", self.init);

opts.observable.on("mainTabChanged", function(idx) {
    self.update({mainTab: idx});
});

opts.observable.on("chartTypeChanged", function() {
    self.update();
});
</script>
</chart-range>

<sequential-chart>
<script>
var self = this;

opts.observable.on("chartSpecChanged", function() {
    if (opts.chartSpec.type === 0)
        self.drawChart();
});

opts.observable.on("chartSizeChanged", function() {
    if (opts.chartSpec.type === 0)
        self.resize();
});

this.header = ["日付", "燃料", "弾薬", "鋼材", "ボーキ", "高速建造材", "高速修復材", "開発資材", "改修資材"];

opts.observable.on("offAllLegends", function() {
    if (opts.chartSpec.type !== 0)
        return;
    self.chart.hide();
    self.header.slice(1).forEach(function(c) {
        self.unselected[c] = true;
    });
});

this.resize = function() {
    if (!self.chart)
        return;
    $('#loading').show();
    setTimeout(function() {
        self.chart.resize(self.chartSize());
    });
};

this.drawChart = function(data) {
    var range = this.calcRange(opts.chartSpec.seqRange);
    if (range.last === 0)
        return;
    if (!data) {
        $('#loading').show();
        $.ajax({
            url: "./資材ログ.json?number=true" +
                "&from=" + range.first + "&to=" + range.last,
            success: function(d) { self.drawChart(d); },
            dataType: "json", cache: false
        });
        return;
    }
    var picked;
    picked = this.pickChartData(data.data, range);
    picked.data.unshift(self.header);
    this.drawSeqChart(picked);
};

this.calcRange = function(range) {
    var first = 0;
    var last = (new Date()).valueOf();
    switch (range) {
        case 0:
            first = moment(last).subtract(24, 'hours').valueOf();
            break;
        case 1:
            first = moment(last).subtract(7, 'days').valueOf();
            break;
        case 2:
            first = moment(last).subtract(1, 'months').valueOf();
            break;
        case 3:
            first = moment(last).subtract(3, 'months').valueOf();
            break;
        case 4:
            break;
        case 5:
            var fromDate = $('#chart_from').datetimepicker("getValue");
            var toDate = $('#chart_to').datetimepicker("getValue");
            if (fromDate === null || toDate === null)
                return {first: 0, last:0};
            first = fromDate.valueOf();
            last = toDate.valueOf();
            break;
    }
    return {first: first, last: last};
};

this.unselected = {};

this.drawSeqChart = function(picked) {
    var size = this.chartSize();
    this.chart = c3.generate({
        bindto: '#chart',
        size: {
            height: size.height,
            width: size.width
        },
        data: {
            x: '日付',
            xFormat: '%Y-%m-%d %H:%M:%S',
            rows: picked.data,
            axes: {
                燃料: 'y',
                弾薬: 'y',
                鋼材: 'y',
                ボーキ: 'y',
                高速建造材: 'y2',
                高速修復材: 'y2',
                開発資材: 'y2',
                改修資材: 'y2'
            }
        },
        point: {
            show: false
        },
        tooltip: {
            show: opts.chartSpec.tooltip
        },
        grid: {
            x: {
                lines: picked.grid
            }
        },
        axis: {
            x: {
                type: 'timeseries',
                tick: {
                    rotate: 30,
                    format: "%m-%d %H:%M",
                    values: picked.tick
                }
            },
            y2: {
                show: true
            }
        },
        legend: {
            item: {
                onclick: function(id) {
                    self.unselected[id] = !self.unselected[id];
                    self.chart.toggle(id);
                }
            }
        },
        onrendered: function() {
            $('#loading').hide();
            opts.observable.trigger("chartRendered");
        }
    });
    self.chart.hide(Object.keys(self.unselected).filter(function(e) {
        return self.unselected[e];
    }));
};

this.pickChartData = function(data, range) {
    var newdata = [];
    var ticks = [];
    var grid = [];
    var first = range.first;
    var last = range.last;
    var interval, tickInterval, lastTick;
    if (last <= first + this.oneDay) {
        interval = 1000;
        tickInterval = 3600 * 1000;
        lastTick = last - last % tickInterval;
    } else if (last <= first + this.oneDay * 21) {
        interval = 1000;
        tickInterval = this.oneDay;
        lastTick = this.to5am(last);
    } else if (last <= first + this.oneDay * 63) {
        interval = 3600 * 1000;
        tickInterval = this.oneDay * 7;
        lastTick = this.to5am(moment(last).day(1).valueOf());
    } else if (last <= first + this.oneDay * 126) {
        interval = 3600 * 6000;
        tickInterval = this.oneDay * 14;
        lastTick = this.to5am(moment(last).day(1).valueOf());
    } else {
        var magn = Math.ceil((last - data[0][0]) / (this.oneDay * 365) / 2);
        interval = this.oneDay * magn;
        tickInterval = this.oneDay * 28 * magn;
        lastTick = this.to5am(moment(last).day(1).valueOf());
    }
    var lastData;
    for (var i = data.length - 1; i >= 0; i--) {
        var row = data[i];
        var date = row[0];
        if (date >= first) {
            if (date <= last) {
                var v = date - date % interval;
                if (lastData !== v) {
                    newdata.unshift(row);
                    lastData = v;
                }
            }
        } else {
            break;
        }
    }
    for (var tick = lastTick; tick > lastData; tick -= tickInterval) {
        var str = self.toDateString(moment(tick));
        ticks.unshift(str);
        grid.unshift({ value: str });
    }
    return { data: newdata, tick: ticks, grid: grid };
};
</script>
</sequential-chart>

<differential-chart>
<script>
var self = this;

opts.observable.on("chartSpecChanged", function() {
    if (opts.chartSpec.type === 1)
        self.drawChart();
});

opts.observable.on("chartSizeChanged", function() {
    if (opts.chartSpec.type === 1)
        self.resize();
});

this.header = ["日付", "燃料", "弾薬", "鋼材", "ボーキ"];

opts.observable.on("offAllLegends", function() {
    if (opts.chartSpec.type !== 1)
        return;
    self.chart.hide();
    self.header.slice(1).forEach(function(c) {
        self.unselected[c] = true;
    });
});

this.resize = function() {
    if (!self.chart)
        return;
    $('#loading').show();
    setTimeout(function() {
        self.chart.resize(self.chartSize());
    });
};

this.drawChart = function(data) {
    var range = this.calcRange(opts.chartSpec.diffRange);
    if (range.last === 0)
        return;
    if (!data) {
        $('#loading').show();
        $.ajax({
            url: "./資材ログ.json?number=true" +
                "&from=" + range.first + "&to=" + range.last,
            success: function(d) { self.drawChart(d); },
            dataType: "json", cache: false
        });
        return;
    }
    var picked;
    picked = this.pickChartData(data.data, range);
    picked.data.unshift(self.header);
    this.drawDiffChart(picked);
};

this.calcRange = function(range) {
    var first = 0;
    var last = (new Date()).valueOf();
    switch (range) {
        case 0:
            first = moment(last).subtract(1, 'months').valueOf();
            break;
        case 1:
            first = moment(last).subtract(3, 'months').valueOf();
            break;
        case 2:
            first = moment(last).subtract(6, 'months').subtract(1, 'weeks').valueOf();
            break;
        case 3:
            break;
        case 4:
            var fromDate = $('#chart_from').datetimepicker("getValue");
            var toDate = $('#chart_to').datetimepicker("getValue");
            if (fromDate === null || toDate === null)
                return {first: 0, last: 0};
            first = Math.max(first, fromDate.valueOf());
            last = Math.min(last, toDate.valueOf());
            break;
    }
    return {first: first, last: last};
};

this.unselected = {};

this.drawDiffChart = function(picked) {
    var size = this.chartSize();
    this.chart = c3.generate({
        bindto: '#chart',
        size: {
            height: size.height,
            width: size.width
        },
        data: {
            x: '日付',
            rows: picked.data,
            axes: {
                燃料: 'y',
                弾薬: 'y',
                鋼材: 'y',
                ボーキ: 'y'
            },
            type: 'bar',
            groups: [["燃料", "弾薬", "鋼材", "ボーキ"]]
        },
        bar: {
            width: {
                ratio: picked.width
            }
        },
        tooltip: {
            show: opts.chartSpec.tooltip
        },
        grid: {
            x: {
                lines: picked.grid
            },
            y: {
                lines: [
                    { value: 0 }
                ]
            }
        },
        axis: {
            x: {
                type: 'timeseries',
                tick: {
                    rotate: 30,
                    format: picked.monthly ? "%Y-%m" : "%m-%d %H:%M",
                    values: picked.tick
                }
            }
        },
        legend: {
            item: {
                onclick: function(id) {
                    self.unselected[id] = !self.unselected[id];
                    self.chart.toggle(id);
                }
            }
        },
        onrendered: function() {
            $('#loading').hide();
            opts.observable.trigger("chartRendered");
        }
    });
    self.chart.hide(Object.keys(self.unselected).filter(function(e) {
        return self.unselected[e];
    }));
};

this.pickChartData = function(data, range) {
    var newdata = [];
    var ticks = [];
    var grid = [];
    var first = range.first;
    var last = range.last;
    var interval, tickInterval, lastTick;
    var barWidth;
    if (first === 0)
        return this.pickMonthlyChartData(data);
    if (last <= first + this.oneDay * 2 * 31) {
        interval = this.oneDay;
        tickInterval = this.oneDay * 2;
        lastTick = this.to5am(last);
        barWidth = 0.3;
    } else if (last <= first + this.oneDay * 3 * 31) {
        interval = this.oneDay;
        tickInterval = this.oneDay * 7;
        lastTick = this.to5am(last);
        barWidth = 0.1;
    } else {
        interval = this.oneDay * 7;
        tickInterval = this.oneDay * 28;
        lastTick = this.to5am(moment(last).day(1).valueOf());
        barWidth = 0.1;
        if (last <= first + this.oneDay * 6 * 38) {
            tickInterval = this.oneDay * 14;
            barWidth = 0.3;
        }
    }
    var lastDate = lastTick;
    var prevRow;
    for (var i = data.length - 1; i >= 0; i--) {
        var row = data[i];
        var date = row[0];
        if (date > first) {
            if (date <= last) {
                if (!prevRow) {
                    prevRow = row;
                    continue;
                }
                if (date <= lastDate) {
                    var newrow = [lastDate];
                    for (var r = 1; r < 5; r++) {
                        newrow.push(prevRow[r] - row[r]);
                    }
                    newdata.unshift(newrow);
                    lastDate = lastDate - interval;
                    prevRow = row;
                }
            }
        } else {
            break;
        }
    }
    if (tickInterval >= this.oneDay * 7)
        lastTick = moment(lastTick).day(1).hour(5).minute(0).valueOf();
    for (var tick = lastTick; tick > lastDate; tick -= tickInterval) {
        ticks.unshift(tick);
        grid.unshift({ value: tick });
    }
    return { data: newdata, tick: ticks, grid: grid, width: barWidth };
};

this.pickMonthlyChartData = function(data) {
    var newdata = [];
    var ticks = [];
    var grid = [];
    var prevRow;
    var prevMonth;
    var row;
    var date;
    for (var i = data.length - 1; i >= 0; i--) {
        row = data[i];
        if (!prevRow) {
            prevRow = row;
            var eom = moment(row[0]).endOf('month');
            prevRow[0] = eom.valueOf();
            prevMonth = eom.month();
            continue;
        }
        date = new Date(row[0]);
        if (prevMonth !== date.getMonth()) {
            var newrow = [prevRow[0]];
            for (var r = 1; r < 5; r++)
                newrow.push(prevRow[r] - row[r]);
            newdata.unshift(newrow);
            ticks.unshift(prevRow[0]);
            grid.unshift({ value: prevRow[0] });
            prevRow = row;
            prevMonth = date.getMonth();
        }
    }
    if (prevRow && date !== prevRow[0]) {
        newrow = [prevRow[0]];
        for (r = 1; r < 5; r++)
            newrow.push(prevRow[r] - row[r]);
        newdata.unshift(newrow);
        ticks.unshift(prevRow[0]);
        grid.unshift({ value: prevRow[0] });
    }
    return { monthly: true, data: newdata, tick: ticks, grid: grid, width: 0.5 };
};
</script>
</differential-chart>

<material-chart>
<div show={mainTabs[mainTab] === "資材グラフ"}>
<span class="c3-legend-item" id="off-all-legends" style="text-decoration: underline; cursor: pointer; z-index: 10; position: absolute; display: none;" onclick={offAllLegends} >全解除</span>
<div id="chart" style="clear: both; margin: 1em;"></div>
</div>

<script>
this.mainTab = 0;
var self = this;

opts.observable.on("mainTabChanged", function(idx) {
    self.update({mainTab: idx});
    if (self.mainTabs[idx] === "資材グラフ")
        opts.observable.trigger("chartSpecChanged");
});

opts.observable.on("chartRendered", function() {
    var legend, offset;
    if (opts.chartSpec.type === 0) {
        legend = $(".c3-legend-item-改修資材>text").offset();
        offset = 80;
    } else {
        legend = $(".c3-legend-item-ボーキ>text").offset();
        offset = 60;
    }
    if (legend)
        $("#off-all-legends").offset({top: legend.top, left: legend.left + offset}).show();
});

this.offAllLegends = function() {
    opts.observable.trigger("offAllLegends");
};

this.timer = null;
$(window).resize(function() {
    if (self.timer)
        clearTimeout(self.timer);
    self.timer = setTimeout(function() {
        if (self.mainTabs[self.mainTab] === "資材グラフ")
            opts.observable.trigger("chartSizeChanged");
        else if (self.mainTabs[self.mainTab] === "戦果")
            opts.observable.trigger("achivementChartSizeChanged");
    }, 200);
});
</script>
</material-chart>

<achivement-table>
<div show={mainTabs[mainTab] === "戦果"}>
<span style="margin-left: 1em;">期間:&nbsp;</span><select style="width: 7em; margin-bottom: 1em;" name="月" onchange={monthChange}>
<option each={m, i in months} value={m}>{m}</option>
</select>
<table id="achivement_table" class="display compact cell-border">
<thead>
<tr><th>日付</th><th>戦果</th><th>EO</th><th>月毎</th></tr>
</thead>
</table>
<div id="achivementChart" style="margin: 1em;"></div>
</div>

<script>
this.on("mount", function() {
    $("#achivement_table").dataTable({
        destroy: true,
        deferRener: true,
        stateSave: true,
        order: [[0, "desc"]],
        paging: false,
        searching: false,
        info: false,
        drawCallback: function() {
            $('#loading').hide();
        }
    });
});

var self = this;

opts.observable.on("mainTabChanged", function(idx) {
    self.update({mainTab: idx});
    if (self.mainTabs[self.mainTab] === "戦果")
        self.updateData();
});

this.months = [];
this.selectedIndex = 0;

this.monthChange = function(event) {
    this.selectedIndex = event.target.selectedIndex;
    if (this.selectedIndex === 0) {
        this.updateData();
        return;
    }
    this.show();
};

this.calcResult = function(data) {
    this.result = {};
    var expPerAch = 10000 / 7.0;
    var dayEo = 0;
    var endOfMonth = moment(0);
    var monthExp = 0;
    var monthEo = 0;
    var endOfYear = moment(0);
    var yearExp = 0;
    var carryOverAch = 0;
    var carryOverEo = 0;
    var prevExp = null;
    var lastDate = moment(0);
    var lastExp = -1;
    var nextDate = moment(0);
    for (var i = 0; i < data.length; i++) {
        var row = data[i];
        var date = this.parseDate(row[0]);
        var exp = row[1] - 0;
        var eo = row[2] - 0;
        var isNewYear = date.isSameOrAfter(endOfYear);
        var isNewMonth = date.isSameOrAfter(endOfMonth);
        var isNewDate = date.isSameOrAfter(nextDate);
        if (isNewDate || isNewMonth || isNewYear) {
            if (lastDate.add(1, 'hours').isSameOrBefore(date)) {
                // 2時を過ぎて最初のexpを戦果の計算に使うと、2時をまたいだ出撃の戦果が前日に加算される。
                // そこで2時前のexpを使って戦果を計算するが、2時前のexpが正しく出力されていない場合は
                // 戦果を正しく計算できない。記録の間隔が1時間以上空いているときは、2時をまたいだ出撃が
                // 行われていない可能性が高いので計算には今のexpを使うことにする。
                // これは5時基準で出力された過去のデータで、妥当な戦果を計算するために必要な処理である。
                lastExp = exp;
            }
            if (nextDate.valueOf() !== 0) {
                var d = isNewDate ? nextDate.subtract(1, 'days') : endOfMonth;
                var m = d.format("YYYY-MM");
                if (!this.result[m])
                    this.result[m] = [];
                this.result[m].push([
                    d.format("YYYY-MM-DD"),
                    ((lastExp - prevExp) / expPerAch).toFixed(1), dayEo,
                    ((lastExp - monthExp) / expPerAch + monthEo + carryOverAch + carryOverEo).toFixed(1)
                ]);
            }
            prevExp = lastExp === -1 ? exp : lastExp;
            if (isNewYear) {
                endOfYear = date.clone().endOf('year').hour(22).startOf('hour');
                if (endOfYear.isSameOrBefore(date))
                    endOfYear.add(1, 'year');
                yearExp = lastExp === -1 ? exp : lastExp;
                monthEo = 0;
            }
            if (isNewMonth) {
                endOfMonth = date.clone().endOf('month');
                if (date.date() === endOfMonth.date())
                    endOfMonth.add(1, 'months').endOf('month');
                endOfMonth.hour(22).startOf('hour');
                monthExp = lastExp === -1 ? exp : lastExp;
                carryOverEo = monthEo * expPerAch / 50000;
                carryOverAch = (monthExp - yearExp) / 50000;
                monthEo = 0;
                m = endOfMonth.format("YYYY-MM");
                if (!this.result[m])
                    this.result[m] = [];
                this.result[m].push([endOfMonth.format("YYYY-MM 引継"),
                    carryOverAch.toFixed(1), carryOverEo.toFixed(1), (carryOverAch + carryOverEo).toFixed(1)]);
            }
            dayEo = 0;
            nextDate = date.clone().hour(2).startOf('hour');
            if (date.hour() >= 2)
                nextDate.add(1, 'days');
            if (nextDate.date() === 1)
                nextDate.add(1, 'days');
        }
        if (date.isBefore(date.clone().endOf('month').hour(22).startOf('hour'))) {
            // 月末22時から翌0時までのEOのボーナス戦果は消える。
            dayEo += eo;
            monthEo += eo;
        }
        lastDate = date;
        lastExp = exp;
    }
};

this.calcChartData = function() {
    this.chartData = {};
    for (var month in this.result) {
        var data = this.chartData[month] = [];
        var result = this.result[month];
        var eo = 0;
        var d = 0;
        data.push(["日付", "戦果", "EO", "月毎"]);
        for (var i = 0; i < result.length; i++) {
            var row = result[i];
            if (row[0].match(/引継/)) {
                eo = row[2] - 0;
                data.push([0, row[1], row[2], row[3]]);
                continue;
            }
            d = moment(row[0], "YYYY-MM-DD").date();
            eo += row[2];
            var ach = (row[3] - eo).toFixed(1);
            data.push([d, ach, eo, row[3]]);
        }
        var endOfMonth = moment(month, "YYYY-MM").endOf("month").date();
        while (d < endOfMonth) {
            d++;
            data.push([d, null, null, null]);
        }
    }
};

this.chartSize = function() {
    var width = Math.max($(window).width() - 6 * this.pxPerEm, 800);
    return {
        height: width * 0.4,
        width: width
    };
};

opts.observable.on("achivementChartSizeChanged", function() {
    if (!self.chart)
        return;
    $('#loading').show();
    setTimeout(function() {
        self.chart.resize(self.chartSize());
    });
});

this.showChart = function(month) {
    this.chart = c3.generate({
        bindto: "#achivementChart",
        size: this.chartSize(),
        data: {
            x: "日付",
            rows: this.chartData[month],
            types: {
                戦果: "area",
                EO: "area",
                月毎: "area"
            }
        },
        onrendered: function() { $('#loading').hide(); }
    });
};

this.updateData = function(data) {
    if (!data) {
        $('#loading').show();
        $.ajax({
            url: "./戦果.json",
            success: function(data) {
                self.updateData(data.data);
            },
            dataType: 'json',
            cache: false
        });
        return;
    }
    this.calcResult(data);
    this.calcChartData();
    this.months = Object.keys(this.result).sort(function(a, b) {
        if (a === b)
            return 0;
        if (a < b)
            return 1;
        return -1;
    });
    this.update();
    this.show();
};

this.show = function() {
    if (this.result === undefined){
        this.updateData();
        return;
    }
    var dt = $('#achivement_table').DataTable();
    dt.clear();
    dt.rows.add(this.result[this.months[this.selectedIndex]]).draw();
    this.showChart(this.months[this.selectedIndex]);
};
</script>
</achivement-table>

<sortie-stat>
<div show={mainTabs[mainTab] === "出撃統計"}>

<ul class="tab tabsub" style="float: left; margin-right: 0.2em">
<li each={tabs} class={select: parent.type === type} onclick={parent.changeTab}>{label}</li>
</ul>

<div style="padding: 0.2em 0;">
<input type="text" id="sortie_stat_from" style="width: 10em">～<input type="text" id="sortie_stat_to" style="width: 10em">
</div>

<div style="clear: both;" show={type === "recent"}>
<h3>今日</h3>
<table id="sortie_stat_day">
</table>
<h3>今週</h3>
<table id="sortie_stat_week">
</table>
<h3>今月</h3>
<table id="sortie_stat_month">
</table>
</div>

<div show={type === "range"}>
<table id="sortie_stat_all" style="width: 100%;">
</table>
</div>

</div>

<script>
this.tabs = [
    {
        type: "recent",
        label: "直近"
    },
    {
        type: "range",
        label: "期間指定"
    }
];
this.type = "recent";
this.changeTab = function(e) {
    this.type = e.item.type;
    this.show();
}.bind(this);

this.mainTab = 0;
var self = this;

this.on("mount", function() {
    $("[id^=sortie]").addClass('display compact cell-border');
    this.init();
});

opts.observable.on("mainTabChanged", function(idx) {
    self.update({mainTab: idx});
    if (self.mainTabs[self.mainTab] === "出撃統計")
        self.show();
});

this.init = function() {
    this.initDatePicker();
};

this.initDatePicker = function() {
    $('#sortie_stat_from').datetimepicker({
        onClose: function() { if (self.type === "range") self.show(); }
    });
    $('#sortie_stat_to').datetimepicker({
        onClose: function() { if (self.type === "range") self.show(); }
    });
};

var self = this;

this.loadData = function() {
    var from, to;
    if (this.type === "recent") {
        from = moment().subtract(1, 'months').subtract(1, 'day').valueOf();
        to = new Date().valueOf();
    } else {
        var fromDate = $('#sortie_stat_from').datetimepicker("getValue");
        var toDate = $('#sortie_stat_to').datetimepicker("getValue");
        if (fromDate === null || toDate === null) {
            this.show([]);
            return;
        }
        from = fromDate.valueOf();
        to = toDate.valueOf();
    }
    $.ajax({
        url: "./海戦・ドロップ報告書.json?from=" + from + "&to=" + to,
        success: function(data) { self.show(data.data); },
        dataType: "json", cache: false
    });
};

this.initResult = function() {
    var now = moment();
    var r;
    if (this.type === "recent") {
        r = {
            day: { stat: {} },
            week: { stat: {} },
            month: { stat: {} }
        };
        r.day.begin = moment(now).hour(5).minute(0).second(0);
        if (now.hour() < 5) {
            r.day.begin.subtract(1, 'days');
        }
        r.week.begin = moment(now).day(1).hour(5).minute(0).second(0);
        if (now.day() === 0 || now.day() === 1 && now.hour() < 5) {
            r.week.begin.subtract(1, 'weeks');
        }
        if (moment(now).endOf('month').date() === now.date() &&
            now.hour() >= 22) { // 月末22時以降
            r.month.begin = moment(now).hour(22).minute(0).second(0);
        } else {
            r.month.begin =
                moment(now).date(1).subtract(1, 'days').
                    hour(22).minute(0).second(0);
        }
    } else {
        r = { all: { stat: {} } };
        r.all.begin = moment(0);
    }
    return r;
};

this.gatherData = function(data) {
    var initStat = function() {
        return { start: "-", S: 0, A: 0, B: 0, C: 0, D: 0, R: 0 };
    };
    var r = this.initResult();
    for (var i = 0; i < data.length; i++) {
        var row = data[i];
        var date = moment(row[0]);
        var map = row[1];
        var isBoss = row[3].indexOf("ボス") !== -1;
        var isStart = row[3].indexOf("出撃") !== -1;
        var resR = 0;
        for (var j = 22; j < row.length; j++) {
            if (/^輸送/.test(row[j]) && /^0\x2f/.test(row[j + 1]))
                resR++;
        }
        var item = /アイテム/.test(row[9]) ? /[^+]+$/.exec(row[10])[0] : null;
        var res = row[4];
        if (res === "E")
            res = "D";
        for (var term in r) {
            if (!r.hasOwnProperty(term))
                continue;
            var to = r[term];
            if (to.begin.isAfter(date))
                continue;
            for (var b = 0; b < 4; b++) {
                var name = b < 2 ? "合計" : map;
                if (b === 1 || b === 3) {
                    if (!isBoss)
                        continue;
                    name = name + " - ボス";
                }
                var mo = to.stat[name];
                if (!mo) {
                    mo = to.stat[name] = initStat();
                    if (name === "合計")
                        to.stat["合計 - ボス"] = initStat();
                }
                mo["R"] += resR;
                mo[res]++;
                if (item) {
                    if (!mo[item])
                        mo[item] = 0;
                    mo[item]++;
                }
                if ((b === 0 || b === 2) && isStart) {
                    if (mo.start === "-")
                        mo.start = 0;
                    mo.start++;
                }
            }
        }
    }
    return r;
};

this.isItemColumn = function(col) {
    return !/^(?:map|start|[SABCDR])$/.test(col);
};

this.sortItemOrder = function(items) {
    ["お米", "梅干", "海苔", "お茶"].reverse().forEach(function(item) {
        var idx = items.indexOf(item);
        if (idx !== -1) {
            items.splice(idx, 1);
            items.unshift(item);
        }
    });
};

this.setupColumns = function(r) {
    for (var term in r) {
        var columns = [{ data: "map", title: "マップ" },
                       { data: "start", title: "出撃" },
                       { data: "S", title: "S" },
                       { data: "A", title: "A" },
                       { data: "B", title: "B" },
                       { data: "C", title: "C" },
                       { data: "D", title: "D" },
                       { data: "R", title: "輸送船" }];
        if (term === "month")
            columns.pop();
        var items = [];
        for (var col in r[term].stat["合計"]) {
            if (this.isItemColumn(col))
                items.push(col);
        }
        this.sortItemOrder(items);
        items.forEach(function(item) {
            columns.push({data: item, title: item});
        });
        r[term].columns = columns;
    }
};

this.fillupItemRecords = function(r) {
    for (var term in r) {
        for (var col in r[term].stat["合計"]) {
            if (!this.isItemColumn(col))
                continue;
            for (var map in r[term].stat) {
                if (map === "合計")
                    continue;
                if (!r[term].stat[map][col]){
                    r[term].stat[map][col] = 0;
                }
            }
        }
    }
};

this.reorderRows = function(r) {
    for (var term in r) {
        if (!r.hasOwnProperty(term))
            continue;
        var table = [];
        var pushed = {};
        for (var map in r[term].stat) {
            if (!r[term].stat.hasOwnProperty(map))
                continue;
            if (pushed[map])
                continue;
            var e = r[term].stat[map];
            e.map = map;
            table.push(e);
            pushed[map] = 1;
            var boss = map + " - ボス";
            e = r[term].stat[boss];
            if (!e)
                continue;
            e.map = boss;
            table.push(e);
            pushed[boss] = 1;
        }
        r[term].table = table;
    }
};

this.show = function(data) {
    if (!data) {
        $('#loading').show();
        this.loadData();
        return;
    }
    var r = this.gatherData(data);
    this.setupColumns(r);
    this.fillupItemRecords(r);
    this.reorderRows(r);
    for (var term in r) {
        var table = $("#sortie_stat_" + term);
        if ($.fn.dataTable.isDataTable(table))
            table.DataTable().destroy();
        table.html("<thead><tr>" +
                   r[term].columns.reduce(function(acc, cur) {
                       return acc + "<th>" + cur.title + "</th>";
                   }, "") + "</tr></thead>");
        table.DataTable({
            paging: false,
            searching: false,
            ordering: false,
            columns: r[term].columns,
            data: r[term].table
        });
    }
    $('#loading').hide();
};
</script>
</sortie-stat>
