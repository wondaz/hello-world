using(['calendar']);
var vms = vms || {};
vms.edit = function (vdata) {
    var self = this;
    this.pkey = vdata.pkey;
    this.form = ko.mapping.fromJS(vdata.form);
    //this.form1 = ko.mapping.fromJS(vdata.form1);    
    this.ds = vdata.dataSource;
    this.urls = vdata.urls;
    delete this.form.__ko_mapping__;
    
    
    this.grid = $.extend({
        size: { w: 6, h: 157 },
        pagination: false,
        remoteSort: false,
        queryParams: ko.observable()
    }, (self.selfGrid || {}));

    self.gridEdit = new com.editGridViewModel(self.grid);
    

    //保存
    this.saveClick = function () {
        self.gridEdit.ended();
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
                } else {
                    com.message('error', '保存失败:' + d.msg);
                }
            }
        });
    };
    
    
    //新增单据
    this.addClick = function () {
        //self.gridEdit.addnew({ CorpID: vdata.corpID });
        com.openTab('客户档案录入', self.urls.edit);
    };
    
    this.gridEdit = new com.editGridViewModel(this.grid);
    this.grid.onDblClickRow = self.gridEdit.begin;
    this.grid.onClickRow = self.gridEdit.ended;
    this.grid.OnAfterCreateEditor = function (editors, row) {
        if (row._isnew == undefined) {
            
        }
            
    };

    //审核、反审核
    this.auditClick = function (vm, event) {
        
    }
};

