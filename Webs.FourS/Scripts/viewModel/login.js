var viewModel = function () {
    var self = this;
    this.form = {
        usercode: ko.observable(),
        password: ko.observable(),
        ip: null,
        city: null
    };
    this.message = ko.observable();
    this.loginClick = function (form) {
        if (!self.form.usercode())
            self.form.usercode($('#usercode').val());

        if (!self.form.password())
            self.form.password($('[type=password]').val());
        $.ajax({
            type: "POST",
            url: "/login/DoLogin",
            data: ko.toJSON(self.form),
            dataType: "json",
            contentType: "application/json",
            success: function (d) {
                if (d.status == 'success') {
                    self.message("登陆成功，正在加载系统...");
                    window.location.href = '/';
                } else {
                    self.message(d.message);
                }
            },
            error: function (e) {
                self.message(e.statusText);
            },
            beforeSend: function () {
                $(form).find("input").attr("disabled", true);
                self.message("正在登陆处理，请稍候...");
            },
            complete: function () {
                $(form).find("input").attr("disabled", false);
            }
        });
    };
};

$(function () { ko.applyBindings(new viewModel());});