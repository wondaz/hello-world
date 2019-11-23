/**
* 模块名：mms viewModel
* 程序名: config.js
* Copyright(c) 2013-2015  [  ] 
**/
 
function viewModel(d) {
    this.form = ko.mapping.fromJS(d.form);
    delete this.form.__ko_mapping__;
    this.save = function () {
        $.ajax({
            url: '/api/sys4s/config',
            type: 'POST',
            contentType: "application/json",
            data: ko.toJSON(this.form),
            success: function (r) {
                com.message('success', '保存成功,请重新登录系统');
            },
            error: function (e) {
                com.message('error', e.responseText);
            }
        });
    };
    return ko.mapping.fromJS(this);
}