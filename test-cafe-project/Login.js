import {Selector} from 'testcafe';

fixture('login test demo')
    .page ('https://securestorage.azurewebsites.net/Login.aspx')

test('Test-user-login-valid', async t => {
    await t
        .typeText('input[name ="ctl00$ContentPlaceHolder1$TextBox1"]', 'krishnakreddy77@gmail.com')
        .typeText('input[name ="ctl00$ContentPlaceHolder1$TextBox2"]', 'passw0rd')
        .click('input[name ="ctl00$ContentPlaceHolder1$Button1"]')
        .expect(Selector('#ContentPlaceHolder1_Label5').innerText).contains('UPLOAD FILE')
});

test('Test-user-login-invalid', async t => {
    await t
        .typeText('input[name ="ctl00$ContentPlaceHolder1$TextBox1"]', 'krishnakreddy77@gmail.com')
        .typeText('input[name ="ctl00$ContentPlaceHolder1$TextBox2"]', 'passw0rd1')
        .setNativeDialogHandler(() => true)
        .click('input[name ="ctl00$ContentPlaceHolder1$Button1"]')
        .expect(Selector('#ContentPlaceHolder1_Label1').innerText).contains('LOGIN')
});

test('Test-user-registration', async t => {
    await t
        .click('a#ContentPlaceHolder1_LinkButton1')
        .typeText('input[name ="ctl00$ContentPlaceHolder1$TextBox1"]', 'kk17')
        .typeText('input[name ="ctl00$ContentPlaceHolder1$TextBox2"]', 'kk.pallamreddy21@outlook.com')
        .typeText('input[name ="ctl00$ContentPlaceHolder1$TextBox3"]', '1234 StreetAvenue')
        .typeText('input[name ="ctl00$ContentPlaceHolder1$TextBox4"]', 'passw0rd')
        .setNativeDialogHandler(() => true)
        .click('input[name ="ctl00$ContentPlaceHolder1$Button1"]')
        .expect(Selector('#ContentPlaceHolder1_Label1').innerText).contains('LOGIN')
});