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

### Bước 10: Kiểm tra và tạo tài khoản mới
Kiểm tra email đã tồn tại trong hệ thống chưa. Nếu chưa, tạo tài khoản mới từ email và thông tin cần thiết.

Lưu ý : hàm login mỗi người sẽ có một cách riêng , nhưng thông thường phổ biến là lưu vào cookie , và session, chúng ta lưu email và role user
Ở đây lưu vào cookie bằng mã token đã mã hóa
Chúng tôi đã sử dụng JWT (JSON Web Token) ( tiêu chuẩn mở được sử dụng để tạo ra các token có thể xác thực được dùng trong việc xác thực và ủy quyền ) và mã hóa token bằng HMACSHA512 (HMAC - Hash-based Message Authentication Code) để ký và xác thực token. 

### Bước 11: Xác thực và đăng nhập
Sử dụng JWT để tạo và mã hóa token, sau đó lưu vào cookie để đăng nhập.

Nếu đăng nhập bằng github thì setting lại biến info username và info email vì thư viện Owin không hỗ trợ github nên chúng ta cần custom lại; Và kiểm tra nếu biến info không tồn tại thì sẽ báo lỗi

Kiểm tra email or username đã tồn tại hay chưa, nếu rồi thì báo lỗi. Còn chưa thì tạo tài khoản mới và sau đó sẽ đăng nhập và về lại trang chủ ( dùng các hàm ở Bước 10 )
