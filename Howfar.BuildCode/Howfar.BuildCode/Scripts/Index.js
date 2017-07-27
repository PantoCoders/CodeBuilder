var nullField = { "Comment": "", "ColumnName": "", "ColumsID": 0, "TypeName": "", "MaxLength": null };
var applist;

(function ($) {
    jQuery.expr[':'].Contains = function (a, i, m) {
        return (a.textContent || a.innerText || "").toUpperCase().indexOf(m[3].toUpperCase()) >= 0;
    };
    function filterList(header, list) {
        var form = $("<form>").attr({ "class": "filterform", "action": "#" }),
            input = $("<input>").attr({ "class": "filterinput form-control", "type": "text" });
        $(form).append(input).appendTo(header);
        $(input).change(function () {
            var filter = $(this).val();
            if (filter) {
                $matches = $(list).find('h4:Contains(' + filter + ')').parent();
                $('a', list).not($matches).slideUp();
                $matches.slideDown();
            } else {
                $(list).find("a").slideDown();
            }
            return false;
        }).keyup(function () {
            $(this).change();
        });
    }
    $(function () {
        $.getJSON('/Ajax/gettablelist', function (data) {
            createVue(data);
            init();
            filterList($("#form"), $("#list"));
        });

    });
}(jQuery));

function init() {
    bindType();
}

function bindType() {
    setTimeout(function () {
        $(".SelectType").autocompleteArray([
            'bigint', 'binary', 'bit', 'char', 'date', 'datetime', 'datetime2', 'datetimeoffset',
            'decimal', 'float', 'geography', 'geometry', 'hierarchyid', 'image', 'int', 'money',
            'nchar', 'ntext', 'numeric', 'nvarchar', 'real', 'smalldatetime', 'smallint', 'smallmoney',
            'sql_variant', 'sysname', 'text', 'time', 'timestamp', 'tinyint', 'uniqueidentifier',
            'varbinary', 'varchar', 'xml'
        ], {
                delay: 10,
                minChars: 0,
                matchSubset: 1,
                width: 400, //提示的宽度
                scrollHeight: 300, //提示的高度
                // onItemSelect:selectItem,
                // onFindValue:findValue,
                autoFill: true,
                maxItemsToShow: 10
            });
    }, 500);
}

function createVue(data) {
    applist = new Vue({
        el: "#list"
        , data: {
            tableList: data,
            fieldList: [nullField]
        }
        , created: function () { }
        , methods: {
            init: function () { },
            getTableInfo: function (e) {
                var element = $(e.currentTarget);
                $('.list-group-item').removeClass('active');
                element.addClass('active');
                var tableName = element.attr('TableName');
                $.getJSON('/Ajax/GetTableDetail?TableName=' + tableName, function (fieldData) {
                    $('#txtTableName').val(tableName);
                    $('#txtEntityName').val(tableName.substr(tableName.indexOf('_') + 1));
                    applist.fieldList = fieldData;
                    bindType();
                });
            },
            addRow: function (index) {
                applist.fieldList.splice(index + 1, 0, {});
            },
            delRow: function (index) {
                applist.fieldList.splice(index, 1);
            }
        }, filters: {},
        computed: {
            computerArr: function () {
                var arr = applist.fieldList;
                return arr.filter(function (item) {
                    console.log(item.ColumnName);
                    $.trim(item.ColumnName).length > 0;
                });
            }
        }
    });
}


function setData(name) {
    var ConfigInfo = {
        TableName: $.trim($('#txtTableName').val()),
        ModelFolderName: $.trim($('#txtModelFolderName').val()),
        EntityName: $.trim($('#txtEntityName').val()),
        PageName: $.trim($('#txtPageName').val())
    };
    console.log(applist.fieldList);
    console.log(applist.computerArr);
    $.post('/Home/SetData', { DataList: applist.fieldList, ConfigInfo: ConfigInfo }, function () {
        document.getElementById('iframe' + name).src = '/Home/' + name;
    });
}
