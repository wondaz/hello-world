﻿/**
* 模块名：easyui方法修改
* 程序名: easyui.fix.js
* Copyright(c) 2013-2015  [  ] 
**/

var easyuifix = {};

/* for easyloader.js ok
 * add after row 13 usage: easyuifix.addLoadModules(_1);
 */
easyuifix.easyloader_addLoadModules = function (modules) {
    $.extend(modules, {
        juidatepick: {
            js: "jquery-ui-datepick.min.js",
            css: "jquery-ui/jquery-ui.css"
        },
        daterange: {
            js: "jquery.daterange.js",
            css: "jquery.daterange.css",
            dependencies: ["juidatepick"]
        },
        extend: {
            js: "extend/jquery.easyui.extend.js"
        },
        lookup: {
            js: "extend/jquery.lookup.js",
            dependencies: ["combo"]
        },
        autocomplete: {
            js: "extend/jquery.autocomplete.js",
            dependencies: ["combo"]
        }
    });
};

easyuifix.easyloader_setting = function (easyloader,src) {
    easyloader.theme = utils.getRequest("theme", src);
    easyloader.locale = utils.getRequest("locale", src);
};

/* for parser.js ok
* add after row 89 usage: easyuifix.parser_addplugins($.parser.plugins);
*/
easyuifix.parser_addplugins = function (plugins) {
    var arr = ["daterange", "lookup", "autocomplete"];
    for (var i in arr) 
        plugins.push(arr[i]);
};

/* for jquery.easyui.min.js 
* add after row 3745 between row [tab.panel("options").tab.remove();] and row [tab.panel("destroy");]
* for clear memory 
* usage: 
*     tab.panel("options").tab.remove();
*     easyuifix.easyui_min_setIframeFree();
*     tab.panel("destroy");
*/
easyuifix.easyui_min_setIframeFree = function (tab) {
    var frame = $('iframe', tab); if (frame.length > 0) { frame[0].contentWindow.document.write(''); frame[0].contentWindow.close(); frame.remove(); if (/msie/.test(navigator.userAgent.toLowerCase())) { CollectGarbage(); } }
}

/* for tabs.js
* add after row 392 _5a.onSelect.call(_58,_5f,_31(_58,_5d));
  usage: easyuifix.tabs_showtabonselect(_5d);
*/
easyuifix.tabs_showtabonselect =function(tab){
    tab.show();    //打开时其它页签先隐藏,,提升用户体验，点击时再显示
}

/* for easyui-lang_zh_CN.js ok
*/
easyuifix.lang_zh_CN = function () {
    if ($.fn.lookup) {
        $.fn.lookup.defaults.missingMessage = '该输入项为必输项';
    }
};

/* for easyui-datagrid.js
* _175 = easyuifix.datagrid_editors_checkboxVal(_173, 174); ok
*/
easyuifix.datagrid_editors_checkboxVal = function (checkbox,value) {
    return (typeof value == 'boolean' && $(checkbox).val() == value.toString());
};

/* for easyui-datagrid.js ok
*/
easyuifix.datagrid_beforeDestroyEditor = function (jq, rowIndex, row) {
    var opts = $.data(jq, "datagrid").options;
    if (opts.OnBeforeDestroyEditor) {
        var editors = {}, list = $(jq).datagrid('getEditors', rowIndex) || [];
        $.each(list, function () { editors[this.field] = this; });
        opts.OnBeforeDestroyEditor(editors, row, rowIndex, jq);
    }
};
/* for easyui-datagrid.js and easyui.min.js  ok
*/
easyuifix.datagrid_afterCreateEditor = function (jq, rowIndex, row) {
    var opts = $.data(jq, "datagrid").options;
    if (opts.OnAfterCreateEditor) {
        var editors = {}, list = $(jq).datagrid('getEditors', rowIndex) || [];
        $.each(list, function () { editors[this.field] = this; });
        opts.OnAfterCreateEditor(editors, row, rowIndex, jq);
    }
};

/* for easyui-combo.js to convert disable to readonly
*/
easyuifix.combo_disableToReadonly = function (jq, b) {
    var options = $.data(jq, "combo").options;
    var combo = $.data(jq, "combo").combo;
    if (b) {
        options.disabled = true;
        $(jq).attr("readonly", true);
        combo.find(".combo-value").attr("readonly", true).addClass("readonly");
        combo.find(".combo-text").attr("readonly", true).addClass("readonly");
    } else {
        options.disabled = false;
        $(jq).removeAttr("readonly");
        combo.find(".combo-value").removeAttr("readonly").removeClass("readonly");
        combo.find(".combo-text").removeAttr("readonly").removeClass("readonly");
    }
};

/* for easyui-spinner.js to convert disable to readonly
*/
easyuifix.spinner_disableToReadonly = function (jq, b) {
    var options = $.data(jq, "spinner").options;
    if (b) {
        options.disabled = true;
        $(jq).attr("readonly", true).addClass("readonly");
    } else {
        options.disabled = false;
        $(jq).removeAttr("readonly").removeClass("readonly");
    }
};


/* for datagrid.js
* add at last if (easyuifix) easyuifix.datagrid_editor_extend(); ok
*/
easyuifix.datagrid_editor_extend = function () {
    if ($.fn.datagrid) {
        if ($.fn.numberspinner) {
            $.extend($.fn.datagrid.defaults.editors, {
                numberspinner: {
                    init: function (container, options) {
                        var input = $('<input type="text">').appendTo(container);
                        return input.numberspinner(options);
                    },
                    destroy: function (target) {
                        $(target).numberspinner('destroy');
                    },
                    getValue: function (target) {
                        return $(target).numberspinner('getValue');
                    },
                    setValue: function (target, value) {
                        $(target).numberspinner('setValue', value);
                    },
                    resize: function (target, width) {
                        $(target).numberspinner('resize', width);
                    }
                }
            });
        }

        if ($.fn.autocomplete) {
            $.extend($.fn.datagrid.defaults.editors, {
                autocomplete: {
                    init: function (container, options) {
                        var input = $('<input type="text" class="z-text datagrid-editable-input">').appendTo(container);
                        return input.autocomplete(options);
                    },
                    destroy: function (target) {
                        $(target).autocomplete('destroy');
                    },
                    getValue: function (target) {
                        return $(target).val();
                    },
                    setValue: function (target, value) {
                        $(target).val(value);
                    },
                    resize: function (target, width) {
                        $(target).width(width);
                    }
                }
            });
        }

        if ($.fn.lookup) {
            $.extend($.fn.datagrid.defaults.editors, {
                lookup: {
                    init: function (container, options) {
                        var input = $('<input type="text" class="z-text datagrid-editable-input">').appendTo(container);
                        return input.lookup(options);
                    },
                    destroy: function (target) {
                        $(target).lookup('destroy');
                    },
                    getValue: function (target) {
                        return $(target).lookup('getValue');
                    },
                    setValue: function (target, value) {
                        $(target).lookup('setValue', value);
                    },
                    resize: function (target, width) {
                        $(target).lookup('resize', width);
                    }
                }
            });
        }

        $.extend($.fn.datagrid.defaults.editors, {
            label: {
                init: function (container, options) {
                    var input = $('<div></div>').appendTo(container);
                    return input;
                },
                destroy: function (target) {

                },
                getValue: function (target) {
                    return $(target).html();
                },
                setValue: function (target, value) {
                    $(target).html(value);
                },
                resize: function (target, width) {

                }
            }
        });
    }
}