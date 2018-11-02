function getRootContainerById(connectionId) {
    var $form = $('body').find('#connectionForm_' + connectionId);
    var $root = $form.closest('.connection-root');
    return $root;
}

function initNewConnection($connRoot, connId) {
    var $form = $connRoot.find('#connectionForm_' + connId);
    var $connType = $form.find('.connection-type');
    var $useDefaultCreds = $form.find('.use-default-credentials');

    onConnectionTypeChange($connType[0]);
    onWindowsIdentityChange($useDefaultCreds[0]);
    
    $form.submit(function (e) {
        e.preventDefault();

        var testModeElem = e.target.elements.TestMode;
        var testMode = testModeElem.value || "false";
        testMode = testMode === "true" ? "true" : "false";
        testModeElem.value = testMode;

        var data = $form.serialize();

        $.post($form[0].action, data, function (savedConnectionView) {
            console.log('data submit complete. test mode? ' + testMode);
            var find = "div.connection-root[data-id='" + connId + "']";
            var toReplace = $('body').find(find);
            toReplace.replaceWith(savedConnectionView);

            var $newForm = $('body').find('#connectionForm_' + connId);
            var $replaced = $newForm.closest('.connection-root');
            initNewConnection($replaced, connId);
        });
    });
}

function onWindowsIdentityChange(element) {
    var checked = element.checked;
    var $root = getRootContainer(element);
    var $username = $root.find('.username');
    var $password = $root.find('.password');

    $username.prop('disabled', checked);
    $password.prop('disabled', checked);

    if (checked) {
        $username.val('');
        $password.val('');
    }
}

function getRootContainer(element) {
    var $element = $(element);
    var $form = $element.closest('.connection-root');
    return $form;
}

function onConnectionTypeChange(element) {
    const typeNtlm = 'TFS Server - NTLM';

    var $root = getRootContainer(element);
    var $defaultCredsRow = $root.find('.default-creds-row');
    var $userCredsRow = $root.find('.user-creds-row');
    var $tokenRow = $root.find('.token-row');
    var type = element.value;
    var $urlExample = $root.find('.url-example');
    var $url = $root.find('.url');

    if (type === typeNtlm) {
        $defaultCredsRow.show();
        $userCredsRow.show();
        $tokenRow.hide();
        $urlExample.html('i.e. http://server:8080/tfs/DefaultCollection');
        $url.attr('placeholder', 'http://server:8080/tfs/DefaultCollection');

        var $user = $userCredsRow.find('.username');
        var $pass = $userCredsRow.find('.password');

        if (!$user.val() && !$pass.val()) {
            var $useDefault = $defaultCredsRow.find('.use-default-credentials');
            $useDefault.prop('checked', true);
            onWindowsIdentityChange($useDefault[0]);
        }
    }
    else {
        $defaultCredsRow.hide();
        $userCredsRow.hide();
        $tokenRow.show();
        $urlExample.html('i.e. <em>https://dev.azure.com/org-name</em> &nbsp; or &nbsp; <em>https://domain.visualstudio.com</em>');
        $url.attr('placeholder', 'https://dev.azure.com/org-name');
    }    
}

function onSavingConnection(form) {
    var $form = $(form);
    var $container = $form.parent();

    $container.find(".tfs-settings-validate-success").hide();

    var $savingLabel = $container.find(".saving-label");
    $savingLabel.show();

    var testMode = form.elements.TestMode.value;

    if (testMode === 'true') {
        console.log('testing connection');
        $savingLabel.html('<p>Testing connection, please wait...</p>');
    }
    else {
        console.log('testing and saving connection');
        $savingLabel.html('<p>Saving and testing connection, please wait...</p>');
    }

    $container.find(".save-button").prop('disabled', true);
    $container.find(".delete-button").prop('disabled', true);
    $container.find(".test-button").prop('disabled', true);
}

function afterSave($form) {
    $form.find(".save-button").prop('disabled', false);
    $form.find(".delete-button").prop('disabled', false);
    $form.find(".delete-button").prop('disabled', true);
    $form.find(".saving-label").hide();
}

function afterDelete(connectionId, result, isError) {
    // it may not remove from backend if never saved before, we remove either way.
    $container = getRootContainerById(connectionId);
    $container.remove();

    var type = 'success';

    if (isError) {
        type = 'danger';
    }
    else if (!result) {
        type = 'info';
    }

    $.notify({
        message: 'Connection removed'
    }, {
        type: type
    });
}

function beginDeleteConnection(element) {
    var $root = getRootContainer(element);
    var $alert = $root.find('.delete-alert');
    $alert.show();
}

function testConnection(element, onSuccess) {
    var $root = getRootContainer(element);
    var $form = $root.find('form');
    $form[0].elements.TestMode.value = 'true';
    $form.submit();
}

function deleteConnection(connectionId) {
    var $root = getRootContainerById(connectionId);
    var $alert = $root.find('.delete-alert');
    $alert.hide();

    $.ajax({
        url: '/settings/remove-connection/' + connectionId,
        type: 'DELETE'
    })
    .done(function (result) {
        afterDelete(connectionId, result, false);
    })
    .fail(function () {
        afterDelete(connectionId, false, true);
    });
}