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
        size: { w: 6, h: 380 },
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
        if (self.form.BillState() == "1") return com.message('warning', '已审核通过，不能修改！');
        var post = {};
        post.head = self.form;
        post.rows = self.gridEdit.getChanges();
        com.ajax({
            url: self.urls.save,
            data: ko.toJSON(post),
            success: function (d) {
                if (d.result) {
                    com.message('success', '保存成功');
                    //vdata.insSaleID = d.xxxx;
                } else {
                    com.message('error', '保存失败:' + d.msg);
                }
            }
        });
    };
    this.loadDetailData = function () {

        self.grid.url = self.urls.search;
        self.grid.queryParams({ _xml: vdata._xml, InsSaleID: vdata.insSaleID });

        com.ajax({
            type: 'GET',
            async: false,
            url: self.urls.archive + self.form.VIN(),
            success: function (d) {
                if (d) {
                    self.form.SignCode(d['SignCode']);
                    self.form.CustomerName(d['CustomerName']);
                    self.form.MobileTel(d['MobileTel']);
                    self.form.FixTel(d['FixTel']);
                    self.form.Postalcode(d['Postalcode']);
                    self.form.CredentialNo(d['CredentialNo']);
                    self.form.Address(d['Address']);
                    self.form.BrandID(d['BrandID']);
                    self.form.SeriesID(d['SeriesID']);
                    self.form.ModelID(d['ModelID']);
                    self.form.EngineCode(d['EngineCode']);
                    self.form.MeasureCode(d['MeasureCode']);
                    self.form.OutsideColor(d['OutsideColor']);
                    self.form.InsideColor(d['InsideColor']);
                    self.form.IsLocal(d['IsLocal']);
                    self.form.IsInstalment(d['IsInstalment']);
                }
            }
        });
    };
    
    
    //新增单据
    this.addClick = function () {
        self.gridEdit.addnew({ CorpID: vdata.corpID });
    };
    
    this.gridEdit = new com.editGridViewModel(this.grid);
    this.grid.onDblClickRow = self.gridEdit.begin;
    this.grid.onClickRow = self.gridEdit.ended;
    this.grid.OnAfterCreateEditor = function (editors, row) {
        if (row._isnew == undefined) {
            
        }
            
    };

    //删除
    this.deleteClick = function () {
        var row = self.grid.datagrid('getSelected');
        if (!row) return com.message('warning', '请选择一条数据');
        if (row["SerialID"] === '' || row["SerialID"]==undefined) {
            self.gridEdit.deleterow();
            return;
        }
        com.message('confirm', '确定删除吗？', function (b) {
            if (b) {
                self.gridEdit.deleterow();
                com.ajax({
                    type: 'Post',
                    url: '/api/biz4s/Insurance/PostDeleteBxMx',
                    data: ko.toJSON(row),
                    success: function (d) {
                        com.message('success', '删除成功！');
                    }
                });
            }
        });
    };

    self.form.VIN.subscribe(function (newValue) {
        if (!newValue) return;

        com.ajax({
            type: 'GET',
            async: false,
            url: self.urls.archive + newValue,
            success: function (d) {
                if (d) {
                    self.form.SignCode(d['SignCode']);
                    self.form.CustomerName(d['CustomerName']);
                    self.form.MobileTel(d['MobileTel']);
                    self.form.FixTel(d['FixTel']);
                    self.form.Postalcode(d['Postalcode']); 
                    self.form.CredentialNo(d['CredentialNo']);
                    self.form.Address(d['Address']); 
                    self.form.BrandID(d['BrandID']);
                    self.form.SeriesID(d['SeriesID']);
                    self.form.ModelID(d['ModelID']);
                    self.form.EngineCode(d['EngineCode']); 
                    self.form.MeasureCode(d['MeasureCode']); 
                    self.form.OutsideColor(d['OutsideColor']);
                    self.form.InsideColor(d['InsideColor']);
                    self.form.IsLocal(d['IsLocal']); 
                    self.form.IsInstalment(d['IsInstalment']);
                }
            }
        });

    });
    //审核、反审核
    this.auditClick = function (vm, event) {
        
    }
    $(function () {
        self.loadDetailData();
    });
    //导出
    this.downloadClick = function (vm, event) {
        var dict = $(event.currentTarget).attr("dict");
        var title = $(document).attr("title");
        com.exporter(self.grid).download('xlsx', dict, title);
    };
};

