using System;
using System.Collections.Generic;

namespace Ontology.GraphInduction.Scoring;

public sealed class DirichletGainModel<TX, TY>
	where TX : notnull
	where TY : notnull
{
	private readonly double _alpha;
	private readonly double _emaAlpha;

	private readonly Dictionary<TY, double> _countY = new();
	private double _totalY;

	private readonly Dictionary<TX, Dictionary<TY, double>> _countXY = new();
	private readonly Dictionary<TX, double> _totalX = new();

	public double GainEma { get; private set; }
	public long Observations { get; private set; }

	public DirichletGainModel(double alpha, double emaAlpha)
	{
		_alpha = alpha;
		_emaAlpha = emaAlpha;
	}

	public void Observe(TX x, TY y)
	{
		Observations++;

		_countY[y] = _countY.TryGetValue(y, out double cy) ? cy + 1 : 1;
		_totalY += 1;

		if (!_countXY.TryGetValue(x, out Dictionary<TY, double>? map))
		{
			map = new Dictionary<TY, double>();
			_countXY[x] = map;
			_totalX[x] = 0;
		}

		map[y] = map.TryGetValue(y, out double cxy) ? cxy + 1 : 1;
		_totalX[x] += 1;

		int k = _countY.Count;
		double py = (_countY[y] + _alpha) / (_totalY + k * _alpha);
		double pyx = (map[y] + _alpha) / (_totalX[x] + k * _alpha);

		double gain = Math.Log(pyx) - Math.Log(py);

		GainEma = _emaAlpha * gain + (1 - _emaAlpha) * GainEma;
	}
}
