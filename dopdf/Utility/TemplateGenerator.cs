using Fluid;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dopdf.Utility
{
    public static class TemplateGenerator
    {
        public static string GetHTMLString()
        {
            var employees = DataStorage.GetAllEmployees();

            var sb = new StringBuilder();
            sb.Append(@"
                        <html>
                            <head>
                            </head>
                            <body>
                                <div class='header'><h1>This is the generated PDF report!!!</h1></div>
                                <table align='center'>
                                    <tr>
                                        <th>Name</th>
                                        <th>LastName</th>
                                        <th>Age</th>
                                        <th>Gender</th>
                                    </tr>");

            foreach (var emp in employees)
            {
                sb.AppendFormat(@"<tr>
                                    <td>{0}</td>
                                    <td>{1}</td>
                                    <td>{2}</td>
                                    <td>{3}</td>
                                  </tr>", emp.Name, emp.LastName, emp.Age, emp.Gender);
            }

            sb.Append(@"
                                </table>
                            </body>
                        </html>");

            return sb.ToString();
        }
        public static string PatchToTmpl(ExpandoObject obj)
        {
            var parser = new FluidParser();
            //var model = new Dictionary<string, object> { { "firstname", 123 }, { "Lastname", DateTime.Now } };
            var source = @"
<html>
<head></head>
<body>
<h1>Hello <b>{{firstname}}</b> {{lastname}}<h1>
<ul>
  {% for item in myarray %}
    <li>
{{item.v1 | prettyprint | paragraph}}. {{item.v2}}
    </li>
  {% endfor %}
</ul>
</body>
</html>
";

            if (parser.TryParse(source, out var template, out var error))
            {
                var context = new TemplateContext(obj);

                return template.Render(context);
            }
            else
            {
                return $"Error: {error}";
            }
        }
    }
}
