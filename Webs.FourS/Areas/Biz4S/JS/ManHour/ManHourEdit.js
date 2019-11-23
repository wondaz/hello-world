using(['calendar']);
var vms = vms || {};
vms.search = function (vdata) {
    var self = this;
    this.pkey = vdata.pkey;
    this.form = ko.mapping.fromJS(vdata.form);
    this.ds = vdata.dataSource;
    this.ds.series = ko.observableArray();
    this.ds.models = ko.observableArray();
    this.urls = vdata.urls;
    delete this.form.__ko_mapping__;

    //保存
    this.saveClick = function () {
       

        if (!com.formValidate()) {
            com.message('warning', '请输入必填项！');
            return;
        }
        
        com.ajax({
            url: self.urls.save,
            data: ko.toJSON(self.form),
            success: function (d) {
                if (d.result) {
                    com.message('success', '保存成功');
                    self.form.ManHourID(d.ManHourID);
                } else {
                    com.message('error', '保存失败:' + d.msg);
                }
            }
        });
    };
};
