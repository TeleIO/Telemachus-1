const baseUrl = 'https://d3kmnwgldcmvsd.cloudfront.net';

class TelemachusMaps {
    layerDefinitions = [
        {
            name: 'Moho Satellite (Stock)',
            layer: this.generateStockLayer({ body: 'moho', style: 'sat' })
        },
        {
            name: 'Moho Biome (Stock)',
            layer: this.generateStockLayer({ body: 'moho', style: 'biome' })
        },
        {
            name: 'Moho Slope (Stock)',
            layer: this.generateStockLayer({ body: 'moho', style: 'slope' })
        },	
        {
            name: 'Eve Satellite (Stock)',
            layer: this.generateStockLayer({ body: 'eve', style: 'sat' })
        },
        {
            name: 'Eve Biome (Stock)',
            layer: this.generateStockLayer({ body: 'eve', style: 'biome' })
        },
        {
            name: 'Eve Slope (Stock)',
            layer: this.generateStockLayer({ body: 'eve', style: 'slope' })
        },
        {
            name: 'Gilly Satellite (Stock)',
            layer: this.generateStockLayer({ body: 'gilly', style: 'sat' })
        },
        {
            name: 'Gilly Biome (Stock)',
            layer: this.generateStockLayer({ body: 'gilly', style: 'biome' })
        },
        {
            name: 'Gilly Slope (Stock)',
            layer: this.generateStockLayer({ body: 'gilly', style: 'slope' })
        },
		{
            name: 'Kerbin Satellite (Stock)',
            layer: this.generateStockLayer({ body: 'kerbin', style: 'sat' })
        },
        {
            name: 'Kerbin Biome (Stock)',
            layer: this.generateStockLayer({ body: 'kerbin', style: 'biome' })
        },
        {
            name: 'Kerbin Slope (Stock)',
            layer: this.generateStockLayer({ body: 'kerbin', style: 'slope' })
        },		
        {
            name: 'Mun Satellite (Stock)',
            layer: this.generateStockLayer({ body: 'mun', style: 'sat' })
        },
        {
            name: 'Mun Biome (Stock)',
            layer: this.generateStockLayer({ body: 'mun', style: 'biome' })
        },
        {
            name: 'Mun Slope (Stock)',
            layer: this.generateStockLayer({ body: 'mun', style: 'slope' })
        },
		{
            name: 'Minmus Satellite (Stock)',
            layer: this.generateStockLayer({ body: 'minmus', style: 'sat' })
        },
        {
            name: 'Minmus Biome (Stock)',
            layer: this.generateStockLayer({ body: 'minmus', style: 'biome' })
        },
        {
            name: 'Minmus Slope (Stock)',
            layer: this.generateStockLayer({ body: 'minmus', style: 'slope' })
        },
		{
            name: 'Duna Satellite (Stock)',
            layer: this.generateStockLayer({ body: 'duna', style: 'sat' })
        },
        {
            name: 'Duna Biome (Stock)',
            layer: this.generateStockLayer({ body: 'duna', style: 'biome' })
        },
        {
            name: 'Duna Slope (Stock)',
            layer: this.generateStockLayer({ body: 'duna', style: 'slope' })
        },		
		{
            name: 'Ike Satellite (Stock)',
            layer: this.generateStockLayer({ body: 'ike', style: 'sat' })
        },
        {
            name: 'Ike Biome (Stock)',
            layer: this.generateStockLayer({ body: 'ike', style: 'biome' })
        },
        {
            name: 'Ike Slope (Stock)',
            layer: this.generateStockLayer({ body: 'ike', style: 'slope' })
        },		
		{
            name: 'Dres Satellite (Stock)',
            layer: this.generateStockLayer({ body: 'dres', style: 'sat' })
        },
        {
            name: 'Dres Biome (Stock)',
            layer: this.generateStockLayer({ body: 'dres', style: 'biome' })
        },
        {
            name: 'Dres Slope (Stock)',
            layer: this.generateStockLayer({ body: 'dres', style: 'slope' })
        },		
        {
            name: 'Laythe Satellite (Stock)',
            layer: this.generateStockLayer({ body: 'laythe', style: 'sat' })
        },
        {
            name: 'Laythe Biome (Stock)',
            layer: this.generateStockLayer({ body: 'laythe', style: 'biome' })
        },
        {
            name: 'Laythe Slope (Stock)',
            layer: this.generateStockLayer({ body: 'laythe', style: 'slope' })
        },
		{
            name: 'Vall Satellite (Stock)',
            layer: this.generateStockLayer({ body: 'vall', style: 'sat' })
        },
        {
            name: 'Vall Biome (Stock)',
            layer: this.generateStockLayer({ body: 'vall', style: 'biome' })
        },
        {
            name: 'Vall Slope (Stock)',
            layer: this.generateStockLayer({ body: 'vall', style: 'slope' })
        },
		{
            name: 'Tylo Satellite (Stock)',
            layer: this.generateStockLayer({ body: 'tylo', style: 'sat' })
        },
        {
            name: 'Tylo Biome (Stock)',
            layer: this.generateStockLayer({ body: 'tylo', style: 'biome' })
        },
        {
            name: 'Tylo Slope (Stock)',
            layer: this.generateStockLayer({ body: 'tylo', style: 'slope' })
        },
		{
            name: 'Bop Satellite (Stock)',
            layer: this.generateStockLayer({ body: 'bop', style: 'sat' })
        },
        {
            name: 'Bop Biome (Stock)',
            layer: this.generateStockLayer({ body: 'bop', style: 'biome' })
        },
        {
            name: 'Bop Slope (Stock)',
            layer: this.generateStockLayer({ body: 'bop', style: 'slope' })
        },
		{
            name: 'Pol Satellite (Stock)',
            layer: this.generateStockLayer({ body: 'pol', style: 'sat' })
        },
        {
            name: 'Pol Biome (Stock)',
            layer: this.generateStockLayer({ body: 'pol', style: 'biome' })
        },
        {
            name: 'Pol Slope (Stock)',
            layer: this.generateStockLayer({ body: 'pol', style: 'slope' })
        },		
		{
            name: 'Eeloo Satellite (Stock)',
            layer: this.generateStockLayer({ body: 'eeloo', style: 'sat' })
        },
        {
            name: 'Eeloo Biome (Stock)',
            layer: this.generateStockLayer({ body: 'eeloo', style: 'biome' })
        },
        {
            name: 'Eeloo Slope (Stock)',
            layer: this.generateStockLayer({ body: 'eeloo', style: 'slope' })
        },
        {
            name: 'Kerbin Satellite (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'kerbin', style: 'sat' })
        },
        {
            name: 'Moho Satellite (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'moho', style: 'sat' })
        },
		{
            name: 'Moho Biome (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'moho', style: 'biome' })
        },
		{
            name: 'Moho Slope (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'moho', style: 'slope' })
        },		
        {
            name: 'Eve Satellite (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'eve', style: 'sat' })
        },
		{
            name: 'Eve Biome (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'eve', style: 'biome' })
        },
		{
            name: 'Eve Slope (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'eve', style: 'slope' })
        },
		{
            name: 'Gilly Satellite (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'gilly', style: 'sat' })
        },
		{
            name: 'Gilly Biome (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'gilly', style: 'biome' })
        },
		{
            name: 'Gilly Slope (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'gilly', style: 'slope' })
        },		
        {
            name: 'Kerbin Satellite (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'kerbin', style: 'sat' })
        },
		{
            name: 'Kerbin Biome (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'kerbin', style: 'biome' })
        },
		{
            name: 'Kerbin Slope (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'kerbin', style: 'slope' })
        },
		{
            name: 'Mun Satellite (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'mun', style: 'sat' })
        },
		{
            name: 'Mun Biome (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'mun', style: 'biome' })
        },
		{
            name: 'Mun Slope (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'mun', style: 'slope' })
        },		
        {
            name: 'Minmus Satellite (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'minmus', style: 'sat' })
        },
		{
            name: 'Minmus Biome (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'minmus', style: 'biome' })
        },
		{
            name: 'Minmus Slope (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'minmus', style: 'slope' })
        },
		{
            name: 'Duna Satellite (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'duna', style: 'sat' })
        },
		{
            name: 'Duna Biome (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'duna', style: 'biome' })
        },
		{
            name: 'Duna Slope (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'duna', style: 'slope' })
        },		
        {
            name: 'Ike Satellite (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'ike', style: 'sat' })
        },
		{
            name: 'Ike Biome (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'ike', style: 'biome' })
        },
		{
            name: 'Ike Slope (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'ike', style: 'slope' })
        },
		{
            name: 'Edna Satellite (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'edna', style: 'sat' })
        },
		{
            name: 'Edna Biome (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'edna', style: 'biome' })
        },
		{
            name: 'Edna Slope (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'edna', style: 'slope' })
        },		
        {
            name: 'Dak Satellite (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'dak', style: 'sat' })
        },
		{
            name: 'Dak Biome (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'dak', style: 'biome' })
        },
		{
            name: 'Dak Slope (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'dak', style: 'slope' })
        },
		{
            name: 'Dres Satellite (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'dres', style: 'sat' })
        },
		{
            name: 'Dres Biome (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'dres', style: 'biome' })
        },
		{
            name: 'Dres Slope (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'dres', style: 'slope' })
        },		
        {
            name: 'Laythe Satellite (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'laythe', style: 'sat' })
        },
		{
            name: 'Laythe Biome (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'laythe', style: 'biome' })
        },
		{
            name: 'Laythe Slope (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'laythe', style: 'slope' })
        },
		{
            name: 'Vall Satellite (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'vall', style: 'sat' })
        },
		{
            name: 'Vall Biome (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'vall', style: 'biome' })
        },
		{
            name: 'Vall Slope (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'vall', style: 'slope' })
        },		
        {
            name: 'Tylo Satellite (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'tylo', style: 'sat' })
        },
		{
            name: 'Tylo Biome (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'tylo', style: 'biome' })
        },
		{
            name: 'Tylo Slope (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'tylo', style: 'slope' })
        },
		{
            name: 'Bop Satellite (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'bop', style: 'sat' })
        },
		{
            name: 'Bop Biome (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'bop', style: 'biome' })
        },
		{
            name: 'Bop Slope (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'bop', style: 'slope' })
        },		
        {
            name: 'Pol Satellite (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'pol', style: 'sat' })
        },
		{
            name: 'Pol Biome (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'pol', style: 'biome' })
        },
		{
            name: 'Pol Slope (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'pol', style: 'slope' })
        },
		{
            name: 'Krel Satellite (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'krel', style: 'sat' })
        },
		{
            name: 'Krel Biome (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'krel', style: 'biome' })
        },
		{
            name: 'Krel Slope (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'krel', style: 'slope' })
        },		
        {
            name: 'Aden Satellite (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'aden', style: 'sat' })
        },
		{
            name: 'Aden Biome (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'aden', style: 'biome' })
        },
		{
            name: 'Aden Slope (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'aden', style: 'slope' })
        },
		{
            name: 'Huygen Satellite (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'huygen', style: 'sat' })
        },
		{
            name: 'Huygen Biome (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'huygen', style: 'biome' })
        },
		{
            name: 'Huygen Slope (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'huygen', style: 'slope' })
        },		
        {
            name: 'Riga Satellite (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'riga', style: 'sat' })
        },
		{
            name: 'Riga Biome (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'riga', style: 'biome' })
        },
		{
            name: 'Riga Slope (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'riga', style: 'slope' })
        },
		{
            name: 'Talos Satellite (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'talos', style: 'sat' })
        },
		{
            name: 'Talos Biome (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'talos', style: 'biome' })
        },
		{
            name: 'Talos Slope (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'talos', style: 'slope' })
        },		
        {
            name: 'Eeloo Satellite (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'eeloo', style: 'sat' })
        },
		{
            name: 'Eeloo Biome (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'eeloo', style: 'biome' })
        },
		{
            name: 'Eeloo Slope (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'eeloo', style: 'slope' })
        },
		{
            name: 'Celes Satellite (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'celes', style: 'sat' })
        },
		{
            name: 'Celes Biome (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'celes', style: 'biome' })
        },
		{
            name: 'Celes Slope (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'celes', style: 'slope' })
        },		
        {
            name: 'Tam Satellite (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'tam', style: 'sat' })
        },
		{
            name: 'Tam Biome (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'tam', style: 'biome' })
        },
		{
            name: 'Tam Slope (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'tam', style: 'slope' })
        },
		{
            name: 'Hamek Satellite (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'hamek', style: 'sat' })
        },
		{
            name: 'Hamek Biome (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'hamek', style: 'biome' })
        },
		{
            name: 'Hamek Slope (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'hamek', style: 'slope' })
        },		
        {
            name: 'Nara Satellite (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'nara', style: 'sat' })
        },
		{
            name: 'Nara Biome (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'nara', style: 'biome' })
        },
		{
            name: 'Nara Slope (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'nara', style: 'slope' })
        },
		{
            name: 'Amos Satellite (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'amos', style: 'sat' })
        },
		{
            name: 'Amos Biome (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'amos', style: 'biome' })
        },
		{
            name: 'Amos Slope (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'amos', style: 'slope' })
        },		
        {
            name: 'Enon Satellite (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'enon', style: 'sat' })
        },
		{
            name: 'Enon Biome (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'enon', style: 'biome' })
        },
		{
            name: 'Enon Slope (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'enon', style: 'slope' })
        },		
        {
            name: 'Prax Satellite (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'prax', style: 'sat' })
        },
		{
            name: 'Prax Biome (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'prax', style: 'biome' })
        },
		{
            name: 'Prax Slope (JNSQ)',
            layer: this.generateJnsqLayer({ service: 'jnsq', body: 'prax', style: 'slope' })
        },        
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
