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

    //审核、反审核 knockoutjs
    this.auditClick = function (vm, event) {
        var vin = self.form.VIN();
        if (!vin) {
            com.message('warning', '车辆的VIN不能为空');
            $('#txtVin').focus();
            return;
        }

        var engine = self.form.EngineCode();
        if (!engine) {
            com.message('warning', '车辆的VIN错误');
            $('#txtVin').focus();
            return;
        }

        if (self.form[self.pkey]() == 0) return;
        var btnStatus = $(event.currentTarget).attr("status");
        var currState = self.form.BillState();
        if (currState == "2") return com.message('warning', '车辆已出库，不能审核！');
        if (btnStatus == "1" && currState == "1") return com.message('warning', '不能重复【审核】！');
        if (btnStatus == "0" && currState == "0") return com.message('warning', '不能【反审核】！');
        var tip = btnStatus == "1" ? "审核" : "反审核";
        var stausName = btnStatus == "1" ? "审核通过" : "审核未通过";
        com.ajax({
            type: 'POST',
            url: vdata.urls.audit,
            data: ko.toJSON({ id: self.form[self.pkey](), status: btnStatus }),
            success: function (d) {
                if (d > 0) {
                    self.form.BillStateName(stausName);
                    self.form.BillState(btnStatus);
                    com.message('success', tip + '成功！');
                } else {
                    com.message('error', tip + '失败：' + d);
                }
            }
        });
    }
    //保存
    this.saveClick = function () {
        if (self.form.BillState() == 1) {
            return com.message('warning', '单据已【审核】，不能修改！');
        }

        if (self.form.BillState() == 2) {
            return com.message('warning', '车辆已【出库】，不能修改！');
        }

        if (!com.formValidate()) {
            com.message('warning', '请输入必填项！');
            return;
        }
        //var vin = self.form.VIN();
        //if (!vin) {
        //    com.message('warning', '请输入车辆的VIN');
        //    $('#txtVin').focus();
        //    return;
        //}

        //var engine = self.form.EngineCode();
        //if (!engine) {
        //    com.message('warning', '车辆的VIN输入错误');
        //    $('#txtVin').focus();
        //    return;
        //}

        //var selSeries = $("#selSeries").get(0);
        //var seriesVal = selSeries.options[selSeries.selectedIndex].value;
        //var selModel = $("#selModel").get(0);
        //var modelVal = selModel.options[selModel.selectedIndex].value;
        //self.form.SeriesID(seriesVal);
        //self.form.ModelID(modelVal);
        //self.form.BillState(0);
        com.ajax({
            url: self.urls.save,
            data: ko.toJSON(self.form),
            success: function (d) {
                if (d.result) {
                    com.message('success', '保存成功');
                } else {
                    com.message('error', '保存失败:' + d.msg);
                }
            }
        });
    };

    self.form.SignCode.subscribe(function (newValue) {
        if (!newValue) return;

        com.ajax({
            type: 'GET',
            async: false,
            url: self.urls.archive + newValue,
            success: function (d) {
                if (d) {
                    self.form.RunDistance(d['RunDistance']);
                    self.form.RepairTel(d['LinkTel']);
                    self.form.RepairName(d['LinkMan']);
                }
            }
        });

    });

    //处理文本框联动数据
    //if (self.form && self.form.SignCode) {
    //    self.form.SignCode.subscribe(function (newValue) {
    //        alert('2252szudjd');
    //        if (!newValue) return;

    //        com.ajax({
    //            type: 'GET',
    //            async: false,
    //            url: self.urls.archive + newValue,
    //            success: function (d) {
    //                if (d) {
    //                    self.form.EngineCode(d['EngineCode']);
    //                    self.form.MeasureCode(d['MeasureCode']);
    //                    self.form.BrandID(d['BrandID']);
    //                    self.form.SeriesID(d['SeriesID']);
    //                    self.form.ModelID(d['ModelID']);
    //                    self.form.OutsideColor(d['OutsideColor']);
    //                    self.form.InsideColor(d['InsideColor']);
    //                }
    //            }
    //        });

    //    });

        

    //    $("#selSeries").change(function () {
    //        $("#selModel").empty();
    //        var firstSel = $("#selSeries").get(0);
    //        var selVal = firstSel.options[firstSel.selectedIndex].value;
    //        if (!selVal) return;

    //        com.ajax({
    //            type: 'GET',
    //            async: false,
    //            url: self.urls.models + selVal,
    //            success: function (d) {
    //                if (d) {
    //                    for (var i = 0; i < d.length; i++) {
    //                        self.ds.models.push({ value: d[i].value, text: d[i].text });
    //                    };
    //                }
    //            }
    //        });
    //    });

    //    ko.computed(function () {
    //        var totalFee = Number(self.form.SalePrice()) - Number(self.form.PreferentialPrice());
    //        self.form.FeeTotal(totalFee);
    //    });
    //}
};
