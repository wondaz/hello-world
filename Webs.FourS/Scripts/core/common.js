/// <reference path="utils.js" />

var com = {};
com.apiRoot = 'http://localhost:1510';
com.upload = 'http://localhost:1510/attachments/';

//格式化时间
com.formatDate = function (value) {
    return utils.formatDate(value, 'yyyy-MM-dd');
};

com.formatTime = function (value) {
    return utils.formatDate(value, 'yyyy-MM-dd hh:mm:ss');
};

//格式化金额
com.formatMoney = function (value) {
    var sign = value < 0 ? '-' : '';
    return sign + utils.formatNumber(Math.abs(value), '#,##0.00');
};

//格式化checkbox
com.formatCheckbox = function (value) {
    var checked = (value || '0').toString() == '1';
    if ((value || 'false').toString() == 'true') {
        checked = '1';
    }
    return utils.formatString('<img value={0} src="/Content/images/{1}"/>', checked, checked ? "checkmark.gif" : "checknomark.gif");
};

//弹messagee
com.message = function (type, message, callback) {
    switch (type) {
        case "success":
            //if (parent == window)
            parent.$('#notity').jnotifyAddMessage({ text: message, permanent: false });
            break;
        case "error":
            if (parent == window) return alert(message);
            parent.$.messager.alert('错误', message);
            break;
        case "warning":
            if (parent == window) return alert(message);
            parent.$('#notity').jnotifyAddMessage({ text: message, permanent: false, type: 'warning' });
            break;
        case "information":
            parent.$.messager.show({
                title: '消息',
                msg: message
                //,showType: 'show'
            });
            break;
        case "confirm":
            return parent.$.messager.confirm('确认', message, callback);
    }

    if (callback) callback();
    return null;
};

com.messageif = function (condition, type, message, callback) {
    if (condition)
        com.message(type, message, callback);
};

com.openTab = function (tabTitle, tabUrl) {
    //parent.wrapper.addTab.apply(this, arguments);
    parent.tabmanager.add({ name: tabTitle, url: tabUrl });
}

com.setLocationHashId = function (id) {
    var hash = parent.location.hash.split('/');
    hash[hash.length - 1] = id;
    parent.location.hash = hash.join('/');
};

com.dmstoken = $.cookie('hbticket');
com.ajax = function (options) {
    options = $.extend({
        showLoading: true
    }, options);

    var ajaxDefaults = {
        type: 'POST',
        dataType: 'json',
        contentType: 'application/json',
        beforeSend: function (XHR) {
            XHR.setRequestHeader('Authorization', 'BasicAuth ' + com.dmstoken);
            if (options.showLoading) {
                $.showLoading();
            };
        },
        error: function (e) {
            var msg = e.responseText;
            var ret = msg.match(/^{\"Message\":\"(.*)\",\"ExceptionMessage\":\"(.*)\",\"ExceptionType\":.*/);
            if (ret != null) {
                msg = (ret[1] + ret[2]).replace(/\\"/g, '"').replace(/\\r\\n/g, '<br/>').replace(/dbo\./g, '');
            }
            else {
                try { msg = $(msg).text() || msg; }
                catch (ex) { }
            }

            com.message('error', msg);
        }
    };

    if (options.showLoading) {
        ajaxDefaults.beforeSend = $.showLoading;
        ajaxDefaults.complete = $.hideLoading;
    }

    $.ajax($.extend(ajaxDefaults, options));
};

com.dataDict = function (url) {
    var ds = [];
    com.ajax({
        type: 'GET',
        async: false,
        url: url,
        success: function (d) {
            ds = d;
        }
    });

    return ds;
}

com.query = function (element) {
    var query = $;
    if ($(document).find(element).length == 0 && element != document) {
        if ($(parent.document).find(element)) {
            query = parent.$;
        }
    }
    return query;
};

com.formValidate = function (context) {
    context = context || document;
    var query = com.query(context);
    if (query.fn.validatebox) {
        var box = query(".validatebox-text", context);
        if (box.length) {
            box.validatebox("validate");
            box.trigger("focus");
            box.trigger("blur");
            var valid = $(".validatebox-invalid:first", context).focus();
            return valid.length == 0;
        }
    }
    return true;
};

com.formChanges = function (obj1, obj2, reserve) {
    obj1 = ko.toJS(obj1) || {};
    obj2 = ko.toJS(obj2) || {};
    reserve = reserve || [];
    var result = utils.diffrence(obj1, obj2, reserve, ['__ko_mapping__']);

    var length = 0;
    for (var k in result) length++;
    result._changed = length > reserve.length;

    return result;
};

com.editGridViewModel = function (grid) {
    var self = this;
    this.begin = function (index, row) {
        if (index == undefined || typeof index === 'object') {
            row = grid.datagrid('getSelected');
            index = grid.datagrid('getRowIndex', row);
        }
        self.editIndex = self.ended() ? index : self.editIndex;
        grid.datagrid('selectRow', self.editIndex).datagrid('beginEdit', self.editIndex);
    };
    this.ended = function () {
        if (self.editIndex == undefined) return true;
        if (grid.datagrid('validateRow', self.editIndex)) {
            grid.datagrid('endEdit', self.editIndex);
            self.editIndex = undefined;
            return true;
        }
        grid.datagrid('selectRow', self.editIndex);
        return false;
    };
    this.addnew = function (rowData) {
        if (self.ended()) {
            if (Object.prototype.toString.call(rowData) != '[object Object]') rowData = {};
            rowData = $.extend({ _isnew: true }, rowData);
            grid.datagrid('appendRow', rowData);
            self.editIndex = grid.datagrid('getRows').length - 1;
            grid.datagrid('selectRow', self.editIndex);
            self.begin(self.editIndex, rowData);
        }
    };
    this.deleterow = function () {
        var selectRow = grid.datagrid('getSelected');
        if (selectRow) {
            var selectIndex = grid.datagrid('getRowIndex', selectRow);
            if (selectIndex == self.editIndex) {
                grid.datagrid('cancelEdit', self.editIndex);
                self.editIndex = undefined;
            }
            grid.datagrid('deleteRow', selectIndex);
        }
    };

    this.deleteCheckedRows = function () {
        var rows = grid.datagrid('getChecked');
        for (var i = 0; i < rows.length; i++) {
            var selectIndex = grid.datagrid('getRowIndex', rows[i]);
            if (selectIndex == self.editIndex) {
                grid.datagrid('cancelEdit', self.editIndex);
                self.editIndex = undefined;
            }
            grid.datagrid('deleteRow', selectIndex);
        }
    };

    this.reject = function () {
        grid.datagrid('rejectChanges');
    };

    this.accept = function () {
        grid.datagrid('acceptChanges');
        var rows = grid.datagrid('getRows');
        for (var i in rows) delete rows[i]._isnew;
    };
    this.getChanges = function (include, ignore) {
        if (!include) include = [], ignore = true;
        var deleted = utils.filterProperties(grid.datagrid('getChanges', "deleted"), include, ignore),
            updated = utils.filterProperties(grid.datagrid('getChanges', "updated"), include, ignore),
            inserted = utils.filterProperties(grid.datagrid('getChanges', "inserted"), include, ignore);

        var changes = { deleted: deleted, inserted: utils.minusArray(inserted, deleted), updated: utils.minusArray(updated, deleted) };
        changes._changed = (changes.deleted.length + changes.updated.length + changes.inserted.length) > 0;

        return changes;
    };
    this.isChangedAndValid = function () {
        return self.ended() && self.getChanges()._changed;
    };
};

com.editTreeGridViewModel = function (grid) {
    var self = this, idField = grid.idField;
    this.begin = function (row) {
        var row = row || grid.treegrid('getSelected');
        if (row) {
            self.editIndex = self.ended() ? row[idField] : self.editIndex;
            grid.treegrid('beginEdit', self.editIndex);
        }
    };
    this.ended = function () {
        if (self.editIndex == undefined) return true;
        if (grid.treegrid('validateRow', self.editIndex)) {
            grid.treegrid('endEdit', self.editIndex);
            self.editIndex = undefined;
            return true;
        }
        grid.treegrid('select', self.editIndex);
        return false;
    };
    this.addnew = function (rowData, parentId) {
        if (self.ended()) {
            if (Object.prototype.toString.call(rowData) != '[object Object]') rowData = {};
            rowData = $.extend({ _isnew: true }, rowData), parentId = parentId || '';
            if (!rowData[idField]) rowData[idField] = utils.uuid();
            grid.treegrid('append', { parent: parentId, data: [rowData] });
            grid.$element().data("datagrid").insertedRows.push(rowData);
            grid.treegrid('select', rowData[idField]);
            self.begin(rowData);
        }
    };
    this.deleterow = function () {
        var row = grid.treegrid('getSelected');
        if (row) {
            if (row[idField] == self.editIndex) {
                grid.treegrid('cancelEdit', self.editIndex);
                self.editIndex = undefined;
            }
            grid.treegrid('remove', row[idField]);
            grid.$element().data("datagrid").deletedRows.push(row);
        }
    };

    this.accept = function () {
        grid.treegrid('acceptChanges');
        var rows = grid.$element().datagrid('getRows');
        for (var i in rows) delete rows[i]._isnew;
    };
    this.getChanges = function (include, ignore) {
        if (!include) include = [], ignore = true;
        var deleted = utils.filterProperties(grid.$element().datagrid('getChanges', "deleted"), include, ignore),
            updated = utils.filterProperties(grid.$element().datagrid('getChanges', "updated"), include, ignore),
            inserted = utils.filterProperties(grid.$element().datagrid('getChanges', "inserted"), include, ignore);

        var changes = { deleted: deleted, inserted: utils.minusArray(inserted, deleted), updated: utils.minusArray(updated, deleted) };
        changes._changed = (changes.deleted.length + changes.updated.length + changes.inserted.length) > 0;

        return changes;
    };
    this.isChangedAndValid = function () {
        return self.ended() && self.getChanges()._changed;
    };
};

com.dialog = function (opts) {
    var query = $, fnClose = opts.onClose;
    opts = query.extend({
        title: 'My Dialog',
        width: 400,
        height: 220,
        closed: false,
        cache: false,
        modal: true,
        html: '',
        url: '',
        viewModel: query.noop
    }, opts);

    opts.onClose = function () {
        if (query.isFunction(fnClose)) fnClose();
        query(this).dialog('destroy');
    };

    if (query.isFunction(opts.html))
        opts.html = utils.functionComment(opts.html);
    else if (/^\#.*\-template$/.test(opts.html))
        opts.html = $(opts.html).html();

    var win = query('<div></div>').appendTo('body').html(opts.html);
    if (opts.url)
        query.ajax({ async: false, url: opts.url, success: function (d) { win.empty().html(d); } });

    win.dialog(opts);
    query.parser.onComplete = function () {
        if ("undefined" === typeof ko)
            opts.viewModel(win);
        else
            ko.applyBindings(new opts.viewModel(win), win[0]);

        query.parser.onComplete = query.noop;
    };
    query.parser.parse(win);
    return win;
};

com.readOnlyHandler = function (type) {
    //readonly
    _readOnlyHandles = {};
    _readOnlyHandles.defaults = _readOnlyHandles.input = function (obj, b) {
        b ? obj.addClass("readonly").attr("readonly", true) : obj.removeClass("readonly").removeAttr("readonly");
    };
    _readOnlyHandles.combo = function (obj, b) {
        var combo = obj.data("combo").combo;
        _readOnlyHandles.defaults(combo.find(".combo-text"), b);
        if (b) {
            combo.unbind(".combo");
            combo.find(".combo-arrow,.combo-text").unbind(".combo");
            obj.data("combo").panel.unbind(".combo");
        }
    };
    _readOnlyHandles.spinner = function (obj, b) {
        _readOnlyHandles.defaults(obj, b);
        if (b) {
            obj.data("spinner").spinner.find(".spinner-arrow-up,.spinner-arrow-down").unbind(".spinner");
        }
    };
    return _readOnlyHandles[type || "defaults"];
};

com.loadCss = function (url, doc, reload) {
    var links = doc.getElementsByTagName("link");
    for (var i = 0; i < links.length; i++)
        if (links[i].href.indexOf(url) > -1) {
            if (reload)
                links[i].parentNode.removeChild(links[i]);
            else
                return;
        }
    var container = doc.getElementsByTagName("head")[0];
    var css = doc.createElement("link");
    css.rel = "stylesheet";
    css.type = "text/css";
    css.media = "screen";
    css.href = url;
    container.appendChild(css);
};

com.exporter = function (opt) {
    var self = this;

    var defaultOptions = {
        action: "/api/sys4s/comm/download",
        dataUrl: "",
        dataParams: {},
        columns: [[]],
        fileType: 'xls'
    };

    this.paging = function (page, rows) {
        self.params.dataParams.page = page;
        self.params.dataParams.rows = rows;
        return self;
    };

    this.title = function (filed, title) {
        self.params.columns[filed] = title;
        return self;
    };

    this.download = function (suffix, codeDict, title) {
        self.params.fileType = suffix || "xls";
        self.params.dataParams = JSON.stringify(self.params.dataParams);
        self.params.columns = JSON.stringify(self.params.columns);
        self.params.codeDict = codeDict || '';
        self.params.title = title || '';
        //创建iframe
        var downloadHelper = $('<iframe style="display:none;" id="downloadHelper"></iframe>').appendTo('body')[0];
        var doc = downloadHelper.contentWindow.document;
        if (doc) {
            doc.open();
            doc.write('')//微软为doc.clear();
            doc.writeln(utils.formatString("<html><body><form id='downloadForm' name='downloadForm' method='post' action='{0}'>", self.params.action));
            for (var key in self.params) doc.writeln(utils.formatString("<input type='hidden' name='{0}' value='{1}'>", key, self.params[key]));
            doc.writeln('<\/form><\/body><\/html>');
            doc.close();
            var form = doc.forms[0];
            if (form) {
                form.submit();
            }
        }
    };

    this.initFromGrid = function (grid) {
        var options = grid.$element().datagrid('options');
        if (grid.treegrid)
            options.url = options.url || grid.treegrid('options').url;

        var columns = [[]], length = Math.max(options.frozenColumns.length, options.columns.length);
        for (var i = 0; i < length; i++)
            columns[i] = (options.frozenColumns[i] || []).concat(options.columns[i] || []);

        self.params = $.extend(true, {}, defaultOptions, {
            dataUrl: com.apiRoot + options.url,
            dataParams: options.queryParams,
            columns: columns
        });
    };

    if (opt.$element)
        self.initFromGrid(opt);
    else
        self.params = $.extend(true, {}, defaultOptions, opt);

    return self;
};

com.setVarible = function (name, value) {
    parent.$(parent.document.body).data(name, value);
};

com.getVarible = function (name) {
    return parent.$(parent.document.body).data(name);
};

com.cookie = $.cookie;

com.getSetting = function (name, defaults) {
    if (!parent.wrapper) return defaults;
    return parent.wrapper.settings[name] || defaults;
};

//导入excel文件时,过滤掉不用的列
com.formatExcelData = function (jsonData, dict) {
    var result = [];
    jsonData.forEach(function (row) {
        var item = {};
        //var dict = { '采购月份': 'ONLINEMONTH', '物料编码': 'MATERIALCODE', '数量': 'MATERIALQTY', '供应商编码': 'SUPPLIERCODE', '备注': 'REMARK', '差额': 'MATERIALQTY' };
        $.each(dict, function (key, value) {
            if (row[key] != null) {
                item[value] = row[key];
            }
        });
        result.push(item);
    });
    return result;
};

com.bindCombobox = function (type) {
    return com.dataDict('/api/sys4s/code/GetDictByType/' + type);
};

com.mergeGridCells = function (dg, arr, condiName) {
    //var dg = $("#gridlist");
    var rowCount = dg.datagrid("getRows").length;
    var cellName;
    var span;
    var perValue = '';
    var curValue = '';
    var perCondition = '';
    var curCondition = '';
    var length = arr.length - 1;
    for (var i = length; i >= 0; i--) {
        cellName = arr[i];
        perValue = "";
        perCondition = "";
        span = 1;
        for (var row = 0; row <= rowCount; row++) {
            if (row == rowCount) {
                curValue = "";
                curCondition = "";
            } else {
                curValue = dg.datagrid("getRows")[row][cellName];
                /* if(cellName=="ORGSTARTTIME"){//特殊处理这个时间字段
					curValue =formatDate(dg.datagrid("getRows")[row][cellName],"");
				} */

                curCondition = dg.datagrid("getRows")[row][condiName];
            }
            if (perValue == curValue && (false || perCondition == curCondition)) {
                span += 1;
            } else {
                var index = row - span;
                dg.datagrid('mergeCells', {
                    index: index,
                    field: cellName,
                    rowspan: span,
                    colspan: null
                });
                span = 1;
                perValue = curValue;
                perCondition = curCondition;
            }
        }
    }
}

//把数据字典的值 改变为文本
com.bindDict = function (dictType) {
    var dictData = com.bindCombobox(dictType);
    return utils.fnValueToText(dictData);
};

com.getDictText = function (type, value) {
    var txt = '';
    com.ajax({
        type: 'GET',
        async: false,
        url: '/api/sys4s/comm/getDictText/',
        data: { type: type, val: value() },
        success: function (d) {
            txt = d;
        }
    });
    return txt;
}

com.bindCalcTotalMoney = function (that, fieldNum, fieldUnitPrice, fieldRowTotal, fieldAllTotal) {
    return function (editors) {
        var RowTotal = editors[fieldRowTotal].target;
        var Numb = editors[fieldNum].target;
        var UnitPrice = editors[fieldUnitPrice].target;

        com.readOnlyHandler('input')(RowTotal.textbox('textbox'), true);
        var calcu = function () {
            var rowTotalMoney = Number(Numb.val()) * Number(UnitPrice.val());
            RowTotal.numberbox('setValue', rowTotalMoney)
            var allMoney = rowTotalMoney - Number(editors[fieldRowTotal].oldHtml.replace(',', '') * 100) / 100;
            $.each(that.grid.datagrid('getData').rows, function () {
                var addMoney = (Number(this[fieldNum]) * Number(this[fieldUnitPrice])) || 0;
                allMoney += addMoney;
            });
            that.form[fieldAllTotal](allMoney);
        }

        Numb.textbox('textbox').blur(calcu);
        UnitPrice.textbox('textbox').blur(calcu);
    }
}

