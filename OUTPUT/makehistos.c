void makehistos()
{
	//static bool loaded = false;
	//if(loaded) gROOT->ProcessLine(".U AtlasStyle.C");
	gROOT->LoadMacro("AtlasStyle.C");  
	SetAtlasStyle();
	
	TH1I *rangehisto;
	TH1D *enobhisto;
	TH1D *sfdrhisto;
	TH1D *sinadhisto;
	TH1D *snrhisto;

	rangehisto = new TH1I("rangeHisto", "Nevis14 Dynamic Range Distribution", 28, 3500, 4000);
	enobhisto = new TH1D("enobHisto", "Nevis14 ENOB Distribution", 40, 9.3, 10.5);
	sfdrhisto = new TH1D("sfdrHisto", "Nevis14 SFDR Distribution", 40, -77.0, -65.0);
	sinadhisto = new TH1D("sinadHisto", "Nevis14 SINAD Distribution", 40, -65.0, -59.0);
	snrhisto = new TH1D("snrHisto", "Nevis14 SNR Distribution", 40, -65.0, -59.0);

	Int_t range = 0;
	Double_t enob, sfdr, sinad, snr;
	
	ifstream qadatafile;
	qadatafile.open("C:\\Users/Max Beck/My Documents/Physics/Chip Testing/Nevis14/OUTPUT/QAparams_rootformat.txt");
	
	int k = 0;
	int j = 0;
	while (!qadatafile.eof())
	{
		qadatafile >> range >> enob >> sfdr >> sinad >> snr;
		if (range > 3500 && range < 4096)
			rangehisto->Fill(range);
		else
			k++;
		if (enob > 9.3)
			enobhisto->Fill(enob);
		else
			j++;
		sfdrhisto->Fill(sfdr);
		sinadhisto->Fill(sinad);
		snrhisto->Fill(snr);
	}
	cout << k << ", " << j;
	TCanvas *rangecanvas = new TCanvas("rangecanvas", "Nevis14 Dynamic Range Distribution", 800, 400);
	rangecanvas->SetFillColor(0);
	rangehisto->SetFillColor(4);
	rangehisto->GetYaxis()->SetTitle("Frequency");
	rangehisto->GetXaxis()->SetTitle("Range");
	rangehisto->Draw();
	rangecanvas->Update();
	//showHistogram(rangehisto);
	showHistogram(enobhisto);
	showHistogram(sinadhisto);
	showHistogram(sfdrhisto);
	showHistogram(snrhisto);
}

void showHistogram(TH1 *h1)
{
	TCanvas *hcanvas = new TCanvas(h1->GetName(), "Distribution", 800, 400);
	hcanvas->SetFillColor(0);
	h1->SetFillColor(4);
	h1->GetYaxis()->SetTitle("Frequency");
	h1->GetXaxis()->SetTitle(h1->GetName());
	h1->Draw();
	hcanvas->Update();
}
	