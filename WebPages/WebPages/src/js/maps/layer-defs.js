const baseUrl = 'https://d3kmnwgldcmvsd.cloudfront.net';

class TelemachusMaps {
    layerDefinitions = [
        {
            name: 'Kerbin Satellite (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'kerbin', style: 'sat' })
        },
        {
            name: 'Kerbin Satellite (Stock)',
            layer: this.generateStockLayer({ body: 'kerbin', style: 'sat' })
        }
    ];

    generateLayer(url, layerDefinition) {
        return new L.tileLayer(`${url}/tiles/${layerDefinition.body}/${layerDefinition.style}/{z}/{x}/{y}.png`, {
            attribution: '&copy; <a href="https://kerbal-maps.finitemonkeys.org/">Kerbal Maps</a>',
            tms: true
        });
    };

    generateJnsqLayer(layerDefinition) {
        return this.generateLayer(`${baseUrl}/jnsq`, layerDefinition);
    };

    generateStockLayer(layerDefinition) {
        return this.generateLayer(baseUrl, layerDefinition);
    };

    layerDefinitionsAsObject() {
        return this.layerDefinitions.reduce((obj, item) => {
            obj[item.name] = item.layer;
            return obj;
        }, {});
    };
}