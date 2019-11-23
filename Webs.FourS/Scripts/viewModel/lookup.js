/**
* 模块名：共通弹出窗口viewModel
* 程序名: lookup.js
**/

function viewModel(data) {
    this.form = {};
    //this.form[data.textField] = data.text;
    //this.form[data.valueField] = data.value;
    this.form[data.textField] = '';
    this.form[data.valueField] = '';
    //this.form["ID"] = '';
    this.textTitle = data.textTitle;
    this.valueTitle = data.valueTitle;
    this.gridSetting = data.grid;
    this.gridSetting.queryParams = "";
    this.searchClick = function () {
        var queryParams = $.extend({ _lookupType: data.lookupType, _r: Math.random() }, data.queryParams, ko.toJS(this.form));
        this.gridSetting.queryParams(queryParams);
    };
    this.clearClick = function () {
        $.each(this.form, function () { this(''); });
        this.searchClick();
    };
    this.confirmClick = function () {
        var rows = $('#list').datagrid('getSelections') || [];
        var txt = '', val = '';
        $.each(rows, function () {
            txt += this[data.textField] + ',';
            val += this[data.valueField] + ',';
            //val += this["ID"] + ',';
        });
        if (txt) txt = txt.substr(0, txt.length - 1);
        if (val) val = val.substr(0, val.length - 1);
        window.returnValue = { value: val, text: txt };
        this.cancelCick();

        return false;
    };
    this.cancelCick = function () {
        data.panel.data("returnValue", window.returnValue);
        data.panel.window('close');
    };
    this.keyDown = function (vm, e) {
        var enterKey = "13", key = e.keyCode || e.which || e.charCode; //兼容IE(e.keyCode)和Firefox(e.which)
        if (key == enterKey) {
            this.searchClick();
            e.preventDefault();
        }
        return true;
    }

    var vm = ko.mapping.fromJS(this);

    vm.gridSetting.queryParams($.extend(data.queryParams, { _lookupType: data.lookupType }));
    return vm;
}

//获取属性
function getOptions() {
    var iframe = getThisIframe();
    var panel = parent.$(iframe).parent();
    var options = $.extend({}, $.fn.lookup.defaults, ko.toJS(panel.data("lookup") || {}), true);
    options.panel = panel;

    //从columns中取得title
    var txtTitle, valTitle;
    options.grid.columns = options.grid.columns || [[]];
    $.each(options.grid.columns[0], function () {
        if (this.field == options.textField)
            txtTitle = this.title;
        if (this.field == options.valueField)
            valTitle = this.title;
    });

    var w = $(window).width() - 48;
    var valueWidth = options.grid.columns[0].length ? 150 : Math.floor(w * 0.3);
    var textWidth = options.grid.columns[0].length ? 150 : Math.floor(w * 0.7);

    //从textField valueField生成columns
    options.grid.columns[0].push({ title: '编码', field: 'CODE', align: 'left', sortable: true, width: valueWidth });
    if (valTitle)
        options.valueTitle = valTitle;
    else
        options.grid.columns[0].push({ field: options.valueField, hidden: true });

    if (txtTitle)
        options.textTitle = txtTitle;
    else
        options.grid.columns[0].push({ title: options.textTitle, field: options.textField, align: 'left', sortable: true, width: textWidth });

    //合并参数
    options.grid = $.extend(options.grid, {
        size: { w: 0, h: 68 },
        url: '/sys4s/plugins/getlookupdata?_r=' + Math.random(),
        pagination: true,
        idField: options.valueField,
        onDblClickRow: function (index, row) {
            row = row || index; //for treegrid dbClickRow(row)
            window.returnValue = { value: row[options.valueField], text: row[options.textField] };
            vmodel.cancelCick();
        },
        singleSelect: !options.multiple
    });

    //扩展树的参数
    if (options.parentField)
        $.extend(options.grid, { pagination: false, treeField: options.valueField, parentField: options.parentField });

    //多选
    if (options.multiple) {
        options.grid.rownumbers = false;
        options.grid.frozenColumns = [[{ field: 'ck', checkbox: true }]];
    }

    return options;
}

//获取本页面所在的iframe
function getThisIframe() {
    if (!parent) return null;
    var iframes = parent.document.getElementsByTagName('iframe');
    if (iframes.length == 0) return null;
    for (var i = 0; i < iframes.length; ++i) {
        var iframe = iframes[i];
        if (iframe.contentWindow === self) {
            return iframe;
        }
    }
}


