using(['panel', 'dialog']);
var vms = vms || {};
vms.edit = function (vdata) {
    var self = this;
    this.urls = vdata.urls;
    this.form1 = ko.mapping.fromJS(vdata.form1);
    this.form = ko.mapping.fromJS(vdata.form);

    this.saveClick = function () {
        if (!com.formValidate()) {
            com.message('warning', '请输入必填项！');
            return;
        }
        var post = {};
        post.head = self.form;
        com.ajax({
            url: self.urls.save,
            data: ko.toJSON(post),
            success: function (d) {
                if (d == "ok") {
                    com.message('success', '保存成功');
                } else {
                    com.message('error', '保存失败:' + d.msg);
                }
            }
        });
    }
    this.auditClick = function () {

    }
    this.auditClick = function () {

    }

    self.form.SaleOrderCode.subscribe(function (newValue) {
        alert(1);
        if (!newValue) return;
        alert(2)
        com.ajax({
            type: 'GET',
            async: false,
            url: self.urls.archive + newValue,
            success: function (d) {
                if (d) {
                    self.form1.SalePrice(d['SalePrice']);
                    self.form1.CustomerName(d['CustomerName']);
                    self.form1.MobileTel(d['MobileTel']);
                    self.form1.Email(d['Email']);
                    self.form1.Address(d['Address']);
                    self.form1.VIN(d['VIN']);
                    self.form1.BrandName(d['BrandName']);
                    self.form1.SeriesName(d['SeriesName']);
                    self.form1.ModelName(d['ModelName']);
                    self.form1.OutsideColor(d['OutsideColor']);
                    self.form1.InsideColor(d['InsideColor']);
                    self.form1.EngineCode(d['EngineCode']);
                    self.form1.MeasureCode(d['MeasureCode']);
                }
            }
        });
    });
}