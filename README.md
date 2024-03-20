# HƯỚNG DẪN SỬ DỤNG OAUTH 2.0 ĐỂ ĐĂNG NHẬP QUA GITHUB TRÊN ASP.NET MVC

## Giới thiệu

Trong dự án này, chúng ta sẽ hướng dẫn cách tích hợp OAuth 2.0 để đăng nhập vào ứng dụng của bạn thông qua Github trên nền tảng ASP.NET MVC. OAuth 2.0 là một giao thức ủy quyền phổ biến được sử dụng cho việc xác thực và ủy quyền trong các ứng dụng web.

## Cài đặt và Cấu hình

### Bước 1: Tải các thư viện cần thiết

Sau khi tạo dự án ASP.NET Framework MVC thành công, hãy truy cập vào NuGet Packet Manager để tải các thư viện sau:
- Microsoft.Owin
- Microsoft.Owin.Security
- Octokit

### Bước 2: Đăng ký Oauth App trên Github

- Truy cập vào [trang đăng ký Oauth App trên Github](https://github.com/settings/developers).
- Chọn **New Oauth App**.
- Điền thông tin cần thiết như sau:
  - **Application name**: Tên của ứng dụng.
  - **Homepage Url**: Đường dẫn đến trang chủ của ứng dụng.
  - **Application Description**: Mô tả ứng dụng (không bắt buộc).
  - **Authorization callback URL**: Địa chỉ URL dùng để chuyển hướng sau khi người dùng cho phép ứng dụng truy cập thông tin từ tài khoản của họ.

### Bước 3: Lấy Client ID và Client Secret từ Github

Sau khi tạo thành công, sao chép **Client ID** và **Client Secret** được sinh ra.

### Bước 4: Cấu hình trong file web.config

Thêm các key vào phần appSetting trong file web.config:
```xml
<add key="RedirectUrl" value="link Authorization callback URL "/>
<add key="ClientIdGH" value="ClientId"/>
<add key="ClientSecretGH" value="ClientSecret"/>
```
### Bước 5: Tạo folder Manager và class Helper
Tạo thư mục Manager và tạo class Helper trong đó để tái sử dụng các hàm.
```CSharp
public partial class Helpers
{
    public static void addCookie(string key, string value, int second = 10)
    {
        HttpCookie cookie = new HttpCookie(key, value);
        cookie.Expires = DateTime.Now.AddSeconds(second);
        HttpContext.Current.Response.Cookies.Add(cookie);
    }

    public static bool IsAuthenticated()
    {
        UserManager userManager = new UserManager();
        return userManager.IsAuthenticated() && userManager.IsUser();
    }

    public static string GetValueFromAppSetting(string key)
    {
        return global::System.Configuration.ConfigurationManager.AppSettings[key];
    }

    public static string UrlGithubLogin()
    {
        string clientIdGh = GetValueFromAppSetting("ClientIdGH");
        string redirectUrl = GetValueFromAppSetting("RedirectUrl");
        return
            "https://github.com//login/oauth/authorize?client_id=" + clientIdGh + "&redirect_uri=" + redirectUrl + "&scope=user:email";
    }
}
```
### Bước 6: Code trong Class Helper
```CSharp
public static string GetValueFromAppSetting(string key)
{
    return global::System.Configuration.ConfigurationManager.AppSettings[key];
}

public static string UrlGithubLogin()
{
    string clientIdGh = GetValueFromAppSetting("ClientIdGH");
    string redirectUrl = GetValueFromAppSetting("RedirectUrl");
    return "https://github.com/login/oauth/authorize?client_id=" + clientIdGh + "&redirect_uri=" + redirectUrl + "&scope=user:email";
}
```
### Bước 7: Thêm hàm GithubLogin() vào Account Controller
Thêm hàm GithubLogin() vào Account Controller để xử lý đăng nhập thông qua Github.
```CSharp
public async Task<ActionResult> GithubLogin(string code)
 {
     var client = new HttpClient();
     var parameters = new Dictionary<string, string>
     {
         { "client_id", ConfigurationManager.AppSettings["ClientIdGH"].ToString() },
         { "client_secret", ConfigurationManager.AppSettings["ClientSecretGH"].ToString() },
         { "code", code },
         { "redirect_uri", ConfigurationManager.AppSettings["RedirectUrl"].ToString() }
     };
     var content = new FormUrlEncodedContent(parameters);
     var response = await    client.PostAsync("https://github.com/login/oauth/access_token", content);
     var responseContent = await response.Content.ReadAsStringAsync();
     var values = HttpUtility.ParseQueryString(responseContent);
     var accessToken = values["access_token"];
     var client1 = new GitHubClient(new Octokit.ProductHeaderValue("Huynguyenjv"));
     var tokenAuth = new Credentials(accessToken);
     client1.Credentials = tokenAuth;
     var user = await client1.User.Current();
     var login = user.Login;
     var provideKey = user.Id.ToString();
     ViewBag.LoginProvider = "Github";
     ViewBag.Email = user.Email;
     ViewBag.login = login;
     ViewBag.provideKey = provideKey;
     return View();
 }
```

### Bước 8: Tạo button đăng nhập Github trong file Login.cshtml
Thêm một button hoặc thẻ <a> để đăng nhập vào Github, sử dụng đường dẫn được tạo bởi hàm UrlGithubLogin().

### Bước 9: Xác nhận thông tin từ Github
Sau khi đồng ý xác nhận với Github, người dùng sẽ được yêu cầu nhập email để xác nhận.
```CsHtml
@model Website_Course_AVG.Models.ExternalLoginConfirmationViewModel
@{
    ViewBag.Title = "Register";
    bool isReadonlyAttribute = ((ViewBag.LoginProvider != "Twitter") && (ViewBag.LoginProvider != "Github"));
}
@section styles{
    
}
<main aria-labelledby="title">
    <h2 class="text-black-75" id="title">@ViewBag.Title.</h2>
    <h3 class="text-black-75">Associate your @ViewBag.LoginProvider account.</h3>

    @using (Html.BeginForm("ExternalLoginConfirmation", "Account", new { ReturnUrl = ViewBag.ReturnUrl }, FormMethod.Post, new { role = "form" }))
    {
        @Html.AntiForgeryToken()

        <h4 class="text-black-75">Association Form</h4>
        @Html.ValidationSummary(true, "", new { @class = "text-danger" })
        <p class="text-danger-50">
            You've successfully authenticated with <strong>@ViewBag.LoginProvider</strong>.
            Please enter a user name for this site below and click the Register button to finish
            logging in.
        </p>
        <div class="row">
            @Html.LabelFor(m => m.Email, new { @class = "col-md-2 col-form-label" })
            <div class="col-md-10">
                <input type="hidden" value="@ViewBag.LoginProvider" name="loginProvider" />
                <input type="hidden" value="@ViewBag.login" name="username" />
                <input type="hidden" value="@ViewBag.provideKey" name="provideKey" />
                <input type="text" name="Email" value="@ViewBag.Email" class="form-control mt-2" @(isReadonlyAttribute ? "readonly" : "") />
            </div>
        </div>
        <div class="row">
            <div class="offset-md-2 col-md-10 d-flex justify-content-center mt-2">
                <input type="submit" class="btn btn-outline-dark" value="Register" />
            </div>
        </div>
    }
</main>

@section Scripts {
    @Scripts.Render("~/bundles/jqueryval")
}

```
### Bước 10: Kiểm tra và tạo tài khoản mới
Kiểm tra email đã tồn tại trong hệ thống chưa. Nếu chưa, tạo tài khoản mới từ email và thông tin cần thiết.
```CSharp
  public bool CheckUsername(string username)
  {
      bool flag = false;
      account account = _data.accounts.Where(x => x.username == username).FirstOrDefault();
      if (account != null)
          flag = true;
      user user = _data.users.Where(x => x.email == username).FirstOrDefault();
      if (user != null)
          flag = true;

      return flag;
  }
```
Tạo User từ email thông tin fullname , username , email
```CSharp
 public async Task<IdentityResult> CreateAccountUserAsync(string fullname, account account, string email)
{
    if (account.username == null)
        return IdentityResult.Failed();

    try
    {
        if (account.password == null)
            account.password = "12345678";
        string password = BCrypt.Net.BCrypt.HashPassword(account.password);
        account.password = password;
        _data.accounts.InsertOnSubmit(account);
        _data.SubmitChanges();

        account accountTmp = _data.accounts.Where(x => x.username == account.username).First();

        user user = new user();
        user.fullname = fullname;
        user.email = email;
        user.account_id = accountTmp.id;
        _data.users.InsertOnSubmit(user);
        _data.SubmitChanges();
        return IdentityResult.Success;
    }
    catch (Exception ex)
    {
        return IdentityResult.Failed();
    }
}
```
Và hàm Login
```CSharp
 public void login(string email)
{
    var token = TokenHelper.GenerateToken(email);
    HttpCookie cookie = new HttpCookie("AuthToken", token);
    cookie.Expires = DateTime.Now.AddDays(30);
    HttpContext.Current.Response.Cookies.Add(cookie);
}
```
Lưu ý : hàm login mỗi người sẽ có một cách riêng , nhưng thông thường phổ biến là lưu vào cookie , và session, chúng ta lưu email và role user
Ở đây lưu vào cookie bằng mã token đã mã hóa
Chúng tôi đã sử dụng JWT (JSON Web Token) ( tiêu chuẩn mở được sử dụng để tạo ra các token có thể xác thực được dùng trong việc xác thực và ủy quyền ) và mã hóa token bằng HMACSHA512 (HMAC - Hash-based Message Authentication Code) để ký và xác thực token. 
```CSharp
public static string GenerateToken(string username, string role = "User")
{
    List<Claim> claims = new List<Claim> {
        new Claim(ClaimTypes.Name, username),
        new Claim(ClaimTypes.Role, role),
    };

    byte[] keyBytes = Encoding.UTF8.GetBytes(Secret);

    int minKeySizeBytes = 64;
    while (keyBytes.Length < minKeySizeBytes)
    {
        keyBytes = keyBytes.Concat(Encoding.UTF8.GetBytes(Secret)).ToArray();
    }

    var key = new SymmetricSecurityKey(keyBytes);

    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

    var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(30),
            signingCredentials: creds
        );

    var jwt = new JwtSecurityTokenHandler().WriteToken(token);

    return jwt;
}
```
### Bước 11: Xác thực và đăng nhập
Sau khi người dùng nhập email đăng kí và ấn xác nhận chúng ta sẽ kiểm tra email đã tồn tại chưa , nếu rồi thì đăng nhập còn chưa sẽ đăng kí tài khoản mới
