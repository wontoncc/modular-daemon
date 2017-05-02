using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace modular_daemon {
    class Config {
        XDocument xml;
        public string Title { get; set;  }
        public List<Service> Services;

        public static Config Load(string filename) {
            var config = new Config() {
                xml = XDocument.Load(@"config.xml")
            };
            var queryTitle = from t in config.xml.Root.Element("application").Element("title").Value
                             select t;
            config.Title = new string(queryTitle.ToArray());
            var queryServices = from s in config.xml.Root.Descendants("services").Elements("service")
                                select s;

            config.Services = new List<Service>();
            foreach (var serviceElem in queryServices) {
                var name = serviceElem.Attribute("name").Value.ToString();
                var command = serviceElem.Element("command").Value.ToString();
                var arguments = serviceElem.Element("arguments")?.Value.ToString();
                var workingDirectory = serviceElem.Element("workingDirectory")?.Value.ToString();
                var log = serviceElem.Element("log")?.Value.ToString();
                config.Services.Add(new Service(name, command, arguments: arguments,
                    dir: workingDirectory,
                    log: log));
            }

            return config;
        }
    }
}
